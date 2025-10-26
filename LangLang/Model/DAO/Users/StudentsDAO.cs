using LangLang.Storage;
using System.Collections.Generic;
using LangLang.Observer;
using LangLang.Model.Users;
using LangLang.Model.Enums;
using System;
using LangLang.Model.DAO.Users.DAOHelpers;
namespace LangLang.Model.DAO.Users
{
    class StudentsDAO
    {
        private readonly List<Student> _students;
        private readonly Storage<Student> _storage;
        private StudentsCourseDAO studentsCourseDAO;
        private StudentsExamDAO studentsExamDAO;
        private StudentsPenaltyDAO studentsPenaltyDAO;
        public Subject StudentSubject;
        public StudentsDAO()
        {
            _storage = new Storage<Student>("students.json");
            _students = _storage.Load();
            StudentSubject = new Subject();
            studentsCourseDAO = new StudentsCourseDAO(this);
            studentsExamDAO = new StudentsExamDAO(this);
            studentsPenaltyDAO = new StudentsPenaltyDAO(this);
        }
        public List<Student> GetAll()=> _students;
        public void Add(Student s) => AddStudent(s);
        public void Update(Student s) => UpdateStudent(s);
        public void Remove(Student s) => RemoveStudent(s.Email);

        public void Load(){
            var fresh = _storage.Load();
            _students.Clear();
            _students.AddRange(fresh);
        }
        public Student AddStudent(Student student)
        {
            _students.Add(student);
            SaveChanges();
            return student;
        }
        public Student? UpdateStudent(Student student)
        {
            Student? oldStudent = GetStudentById(student.Email);
            if (oldStudent is null) return null;
            foreach (var prop in typeof(Student).GetProperties())
            {
                if (prop.CanWrite && prop.Name != nameof(Student.Email))
                {
                    var newValue = prop.GetValue(student);
                    prop.SetValue(oldStudent, newValue);
                }
            }
            SaveChanges();
            return oldStudent;
        }
        public Student? RemoveStudent(string email)
        {
            Student? student = GetStudentById(email);
            if (student == null) return null;
            _students.Remove(student);
            SaveChanges();
            return student;
        }
        public Student? GetStudentById(string email) => _students.Find(s => s.Email == email);
        public List<Student> GetAllStudents() => _students;
        public void SaveChanges()
        {
            _storage.Save(_students);
            StudentSubject.NotifyObservers();
        }
        public List<int> GetRequestedCourses(string email)
        {
            return studentsCourseDAO.GetRequestedCourses(email);
        }
        public List<Tuple<Language, LanguageLevel>> GetFinishedCourses(string email)
        {
            return studentsCourseDAO.GetFinishedCourses(email);
        }
        public void FinishActiveCourse(string studentEmail, Language language, LanguageLevel level)
        {
            studentsCourseDAO.FinishActiveCourse(studentEmail, language, level);
        }
        public void StartCourse(string studentEmail, int courseId)
        {
            studentsCourseDAO.StartCourse(studentEmail, courseId);
        }
        public void RemoveFromCourse(string studentEmail, int courseId)
        {
            studentsCourseDAO.RemoveFromActiveCourse(studentEmail, courseId);
        }
        public void AddCourseRequest(string studentEmail, int courseId)
        {
            studentsCourseDAO.AddCourseRequest(studentEmail, courseId);
        }
        public void RemoveCourseRequest(string studentEmail, int courseId)
        {
            studentsCourseDAO.RemoveCourseRequest(studentEmail, courseId);
        }
        public int ApplyMonthlyPenaltyDecayForAll(DateTime now) => studentsPenaltyDAO.ApplyMonthlyPenaltyDecayForAll(now);
        public void AddPenaltyPoint(string email) => studentsPenaltyDAO.AddPenaltyPoint(email);

        public List<int> GetRequestedExamTerms(string email)
        {
            return studentsExamDAO.GetRequestedExamTerms(email);
        }
        public int GetActiveExamTermId(string email)
        {
            return studentsExamDAO.GetActiveExamTermId(email);
        }
        public List<Tuple<Language, LanguageLevel>> GetFinishedExams(string email)
        {
            return studentsExamDAO.GetFinishedExams(email);
        }
        public void AddExamRequest(string studentEmail, int examTermId)
        {
            studentsExamDAO.AddExamRequest(studentEmail, examTermId);
        }
        public void RemoveExamRequest(string studentEmail, int examTermId)
        {
            studentsExamDAO.RemoveExamRequest(studentEmail, examTermId);
        }
        public void SetActiveExamTerm(string studentEmail, int examTermId)
        {
            studentsExamDAO.SetActiveExamTerm(studentEmail, examTermId);
        }
        public void ClearActiveExamTerm(string studentEmail)
        {
            studentsExamDAO.ClearActiveExamTerm(studentEmail);
        }
        public void FinishActiveExam(string studentEmail, Language language, LanguageLevel level)
        {
            studentsExamDAO.FinishActiveExam(studentEmail, language, level);
        }
        public void LockExamApplications(string studentEmail)
        {
            studentsExamDAO.LockExamApplications(studentEmail);
        }
        public void UnlockExamApplications(string studentEmail)
        {
            studentsExamDAO.UnlockExamApplications(studentEmail);
        }
        public void RecordExamDecision(string studentEmail, Language language, LanguageLevel level, bool passed)
        {
            studentsExamDAO.RecordExamDecision(studentEmail, language, level, passed);
        }
    }
}