using LangLang.Model.Requests;
using LangLang.Observer;
using LangLang.Storage;
using System.Collections.Generic;
namespace LangLang.Model.DAO.Requests
{
    class ExamEnrollmentsDAO{
        private readonly List<ExamEnrollment> _examEnrollments;
        private readonly Storage<ExamEnrollment> _storage;
        public Subject ExamEnrollmentSubject;
        public ExamEnrollmentsDAO(){
            _storage = new Storage<ExamEnrollment>("exam_enrollments.json");
            _examEnrollments = _storage.Load();
            ExamEnrollmentSubject = new Subject();
        }
        public List<ExamEnrollment> GetAll() => _examEnrollments;
        public void Add(ExamEnrollment e) => AddExamEnrollment(e);
        public void Update(ExamEnrollment e) => UpdateExamEnrollment(e);
        public void Remove(ExamEnrollment e){
            if (e == null) return;
            RemoveExamEnrollment(e.ID);
        }
        public bool RemoveById(int id) => RemoveExamEnrollment(id) != null;
        public void SaveChanges() => _storage.Save(_examEnrollments);
        public void Load(){
            var fresh = _storage.Load();
            _examEnrollments.Clear();
            _examEnrollments.AddRange(fresh);
        }
        public int NextId(){
            return _examEnrollments.Count == 0 ? 1 : _examEnrollments[^1].ID + 1;
        }
        public ExamEnrollment AddExamEnrollment(ExamEnrollment examEnrollment){
            int nextId = _examEnrollments.Count > 0 ? _examEnrollments[^1].ID + 1 : 1;
            examEnrollment.ID = nextId;
            _examEnrollments.Add(examEnrollment);
            _storage.Save(_examEnrollments);
            ExamEnrollmentSubject.NotifyObservers();
            return examEnrollment;
        }
        public ExamEnrollment? UpdateExamEnrollment(ExamEnrollment examEnrollment){
            ExamEnrollment? oldExamEnrollment = GetExamEnrollmentById(examEnrollment.ID);
            if (oldExamEnrollment is null) return null;
            oldExamEnrollment.StudentID = examEnrollment.StudentID;
            oldExamEnrollment.TeachingActivityID = examEnrollment.TeachingActivityID;
            oldExamEnrollment.RequestDate = examEnrollment.RequestDate;
            oldExamEnrollment.Status = examEnrollment.Status;
            _storage.Save(_examEnrollments);
            ExamEnrollmentSubject.NotifyObservers();
            return oldExamEnrollment;
        }
        public ExamEnrollment? RemoveExamEnrollment(int id){
            ExamEnrollment? examEnrollment = GetExamEnrollmentById(id);
            if (examEnrollment == null) return null;

            _examEnrollments.Remove(examEnrollment);
            _storage.Save(_examEnrollments);
            ExamEnrollmentSubject.NotifyObservers();
            return examEnrollment;
        }
        public void RemoveEnrollmentsByStudent(string studentId){
            _examEnrollments.RemoveAll(ee => ee.StudentID == studentId);
            _storage.Save(_examEnrollments);
            ExamEnrollmentSubject.NotifyObservers();
        }
        public ExamEnrollment? GetExamEnrollmentById(int id){
            return _examEnrollments.Find(ee => ee.ID == id);
        }
        public List<ExamEnrollment> GetAllExamEnrollments(){
            return _examEnrollments;
        }
        public List<ExamEnrollment> GetByExamTerm(int examTermId){
            return _examEnrollments.FindAll(e => e.TeachingActivityID == examTermId);
        }
    }
}
