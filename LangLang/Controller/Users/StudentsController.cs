using System;
using System.Collections.Generic;
using LangLang.Model.DAO.Users;
using LangLang.Model.Enums;
using LangLang.Model.Users;
using LangLang.Observer;

namespace LangLang.Controller.Users
{
    public class StudentsController
    {
        private readonly StudentsDAO _students;

        public StudentsController()
        {
            _students = new StudentsDAO();
        }

        public List<Student> GetAllStudents()
        {
            return _students.GetAllStudents();
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
            Student? student = GetStudentById(studentEmail);
            if (student != null && !student.RequestedCourses.Contains(courseId))
            {
                student.RequestedCourses.Add(courseId);
                Update(student);
            }
        }
        public void RemoveCourseRequest(string studentEmail, int courseId)
        {
            Student? student = GetStudentById(studentEmail);
            if (student != null && student.RequestedCourses.Contains(courseId))
            {
                student.RequestedCourses.Remove(courseId);
                Update(student);
            }
        }

        public void FinishActiveCourse(string studentEmail, Language language, LanguageLevel level)
        {
            _students.FinishActiveCourse(studentEmail, language, level);
        }

        public void Subscribe(IObserver observer)
        {
            _students.StudentSubject.Subscribe(observer);
        }

        public void StartCourse(string studentEmail, int courseId)
        {
            var student = GetStudentById(studentEmail);
            if (student == null) return;

            if (student.ActiveCourseId != -1) return;

            if (student.RequestedCourses.Contains(courseId))
            {
                student.RequestedCourses.Remove(courseId);
                student.ActiveCourseId = courseId;
                Update(student);
            }
        }
        public int ApplyPenaltyDecayForAll()
        {
            return _students.ApplyMonthlyPenaltyDecayForAll(DateTime.Now);
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

        public bool IsExamApplicationLocked(string studentEmail)
         => _students.GetStudentById(studentEmail)?.IsExamApplicationLocked ?? false;

        public void LockExamApplications(string studentEmail)
            => _students.LockExamApplications(studentEmail);

        public void UnlockExamApplications(string studentEmail)
            => _students.UnlockExamApplications(studentEmail);

        public void RecordExamDecision(string studentEmail, Language language, LanguageLevel level, bool passed)
            => _students.RecordExamDecision(studentEmail, language, level, passed);
    }
}