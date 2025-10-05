using LangLang.Model.TeachingActivities;
using LangLang.Observer;
using LangLang.Storage;
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

        public ExamResultsDAO()
        {
            _storage = new Storage<ExamResult>("exam-results.json");
            _examResults = _storage.Load();
            ExamResultSubject = new Subject();
            _freeIds = RecalculateFreeIds(_examResults);
            _maxId = _examResults.Count == 0 ? 0 : _examResults.Max(t => t.Id);
        }

        private SortedSet<int> RecalculateFreeIds(List<ExamResult> results)
        {
            var free = new SortedSet<int>();
            if (results.Count == 0) return free;
            int maxId = results.Max(r => r.Id);
            var used = new HashSet<int>(results.Select(r => r.Id));
            for (int i = 1; i <= maxId; i++)
                if (!used.Contains(i)) free.Add(i);
            return free;
        }

        private int GenerateId()
        {
            if (_freeIds.Count > 0)
            {
                int id = _freeIds.Min;
                _freeIds.Remove(id);
                return id;
            }
            return ++_maxId;
        }

        public ExamResult AddExamResult(ExamResult result)
        {
            if (result.Id == 0)
                result.Id = GenerateId();

            _examResults.Add(result);
            _storage.Save(_examResults);
            ExamResultSubject.NotifyObservers();
            return result;
        }

        public ExamResult? GetByExamAndStudent(int examId, string studentId)
        {
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
