using LangLang.Model.TeachingActivities;
using LangLang.Observer;
using LangLang.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
namespace LangLang.Model.DAO.TeachingActivity
{
    class ExamResultsDAO
    {
        private readonly List<ExamResult> _examResults;
        private readonly Storage<ExamResult> _storage;
        public Subject ExamResultSubject;
        private readonly SortedSet<int> _freeIds;
        private int _maxId;
        public ExamResultsDAO(){
            _storage = new Storage<ExamResult>("exam-results.json");
            _examResults = _storage.Load();
            ExamResultSubject = new Subject();
            _freeIds = RecalculateFreeIds(_examResults);
            _maxId = _examResults.Count == 0 ? 0 : _examResults.Max(t => t.Id);
        }
        public List<ExamResult> GetAll() => _examResults;
        public void Add(ExamResult r) => AddExamResult(r);
        public void Update(ExamResult r){
            var existing = GetById(r.Id);
            if (existing == null) return;
            var t = typeof(ExamResult);
            foreach (var p in t.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                if (!p.CanRead || !p.CanWrite) continue;
                if (string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase)) continue;

                var newVal = p.GetValue(r);
                p.SetValue(existing, newVal);
            }
            _storage.Save(_examResults);
            ExamResultSubject.NotifyObservers();
        }
        public void Remove(ExamResult r){
            var existing = GetById(r.Id);
            if (existing == null) return;
            _examResults.Remove(existing);
            _freeIds.Add(r.Id);
            _storage.Save(_examResults);
            ExamResultSubject.NotifyObservers();}
        public bool RemoveById(int id){
            var existing = GetById(id);
            if (existing == null) return false;
            _examResults.Remove(existing);
            _freeIds.Add(id);
            _storage.Save(_examResults);
            ExamResultSubject.NotifyObservers();
            return true;}
        public void SaveChanges(){
            _storage.Save(_examResults);
            ExamResultSubject.NotifyObservers();
        }
        public void Load(){
            var fresh = _storage.Load();
            _examResults.Clear();
            _examResults.AddRange(fresh);
            _freeIds.Clear();
            foreach (var id in RecalculateFreeIds(_examResults))
                _freeIds.Add(id);
            _maxId = _examResults.Count == 0 ? 0 : _examResults.Max(x => x.Id);
        }
        public int NextId(){
            if (_freeIds.Count > 0) return _freeIds.Min;
            return _maxId + 1;
        }
        private SortedSet<int> RecalculateFreeIds(List<ExamResult> results){
            var free = new SortedSet<int>();
            if (results.Count == 0) return free;
            int maxId = results.Max(r => r.Id);
            var used = new HashSet<int>(results.Select(r => r.Id));
            for (int i = 1; i <= maxId; i++)
                if (!used.Contains(i)) free.Add(i);
            return free;
        }
        private int GenerateId(){
            if (_freeIds.Count > 0){
                int id = _freeIds.Min;
                _freeIds.Remove(id);
                return id;
            }
            return ++_maxId;
        }
        public ExamResult AddExamResult(ExamResult result){
            if (result.Id == 0)
                result.Id = GenerateId();
            _examResults.Add(result);
            _storage.Save(_examResults);
            ExamResultSubject.NotifyObservers();
            return result;
        }
        public ExamResult? GetByExamAndStudent(int examId, string studentId){
            return _examResults
                .FirstOrDefault(r => r.ExamId == examId && r.StudentId == studentId);
        }
        public List<ExamResult> GetAllExamResults()
            => _examResults;
        public ExamResult? GetById(int id)
            => _examResults.FirstOrDefault(r => r.Id == id);
        public List<ExamResult> GetByStudentId(string studentId)
            => _examResults.Where(r => r.StudentId == studentId).ToList();
    }
}
