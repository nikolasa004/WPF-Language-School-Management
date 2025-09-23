using System;
using System.Collections.Generic;
using LangLang.Model.DAO.Users;
using LangLang.Model.Enums;
using LangLang.Model.Users;

namespace LangLang.Controller.Users
{
    public class StudentsController
    {
        private readonly StudentsDAO _students;

        public StudentsController()
        {
            _students = new StudentsDAO();
        }

        public void Add(Student student)
        {
            _students.AddStudent(student);
        }
        public Student? Update(Student student)
        {
            return _students.UpdateStudent(student);
        }
        public void Delete(string studentEmail)
        {
            _students.RemoveStudent(studentEmail);
        }
        public Student? GetStudentById(string email)
        {
            return _students.GetStudentById(email);
        }
        public List<Student> GetAllStudents()
        {
            return _students.GetAllStudents();
        }

        public List<int> GetRequestedCourses(string email)
        {
            return _students.GetRequestedCourses(email);
        }
        public List<Tuple<Language, LanguageLevel>> GetFinishedCourses(string email)
        {
            return _students.GetFinishedCourses(email);
        }
        public void AddCourseRequest(string studentEmail, int courseId)
        {
            _students.AddCourseRequest(studentEmail, courseId);
        }
        public void RemoveCourseRequest(string studentEmail, int courseId)
        {
            _students.RemoveCourseRequest(studentEmail, courseId);
        }
        public void FinishActiveCourse(string studentEmail, Language language, LanguageLevel level)
        {
            _students.FinishActiveCourse(studentEmail, language, level);
        }
        public void StartCourse(string studentEmail, int courseId)
        {
            _students.StartCourse(studentEmail, courseId);
        }

        public int ApplyPenaltyDecayForAll()
        {
            return _students.ApplyMonthlyPenaltyDecayForAll(DateTime.Now);
        }

        public void RemoveFromCourse(string studentEmail, int courseId)
        {
            var student = GetStudentById(studentEmail);
            if (student == null) return;

            if (student.RequestedCourses.Contains(courseId))
            {
                student.RequestedCourses.Remove(courseId);
            }

            if (student.ActiveCourseId == courseId)
            {
                student.ActiveCourseId = -1;
            }

            Update(student);
        }

        public void AddPenaltyPoint(string studentEmail)
        {
            var student = GetStudentById(studentEmail);
            if (student == null) return;

            student.PenaltyPoints += 1;
            student.LastPenaltyProcessedAt = DateTime.Now.ToString("yyyy-MM-dd");

            Update(student);
        }
        public List<int> GetRequestedExamTerms(string email)
        {
            return _students.GetRequestedExamTerms(email);
        }
        public int GetActiveExamTermId(string email)
        {
            return _students.GetActiveExamTermId(email);
        }
        public List<Tuple<Language, LanguageLevel>> GetFinishedExams(string email)
        {
            return _students.GetFinishedExams(email);
        }
        public void AddExamRequest(string studentEmail, int examTermId)
        {
            _students.AddExamRequest(studentEmail, examTermId);
        }
        public void RemoveExamRequest(string studentEmail, int examTermId)
        {
            _students.RemoveExamRequest(studentEmail, examTermId);
        }
        public void SetActiveExamTerm(string studentEmail, int examTermId)
        {
            _students.SetActiveExamTerm(studentEmail, examTermId);
        }
        public void ClearActiveExamTerm(string studentEmail)
        {
            _students.ClearActiveExamTerm(studentEmail);
        }
        public void FinishActiveExam(string studentEmail, Language language, LanguageLevel level)
        {
            _students.FinishActiveExam(studentEmail, language, level);
        }
        public bool IsExamApplicationLocked(string studentEmail) => _students.GetStudentById(studentEmail)?.IsExamApplicationLocked ?? false;
        public void LockExamApplications(string studentEmail) => _students.LockExamApplications(studentEmail);
        public void UnlockExamApplications(string studentEmail) => _students.UnlockExamApplications(studentEmail);
        public void RecordExamDecision(string studentEmail, Language language, LanguageLevel level, bool passed) => _students.RecordExamDecision(studentEmail, language, level, passed);
    }
}