using LangLang.Model.Enums;
using LangLang.Model.Users;
using System;
using System.Collections.Generic;

namespace LangLang.Model.DAO.Users.DAOHelpers
{
    class StudentsCourseDAO
    {
        private StudentsDAO _studentsDAO;
        public StudentsCourseDAO(StudentsDAO studentsDAO)
        {
            _studentsDAO = studentsDAO;
        }

        public List<int> GetRequestedCourses(string email)
        {
            Student? student = _studentsDAO.GetStudentById(email);
            if (student == null) return new List<int>();
            return student.RequestedCourses;
        }
        public List<Tuple<Language, LanguageLevel>> GetFinishedCourses(string email)
        {
            Student? student = _studentsDAO.GetStudentById(email);
            if (student == null) return new List<Tuple<Language, LanguageLevel>>();
            return student.FinishedCourses;
        }
        public void FinishActiveCourse(string studentEmail, Language language, LanguageLevel level)
        {
            Student? student = _studentsDAO.GetStudentById(studentEmail);
            if (student != null && student.ActiveCourseId != -1)
            {
                student.FinishedCourses.Add(new Tuple<Language, LanguageLevel>(language, level));
                student.ActiveCourseId = -1;
                _studentsDAO.SaveChanges();
            }
        }
        public void StartCourse(string studentEmail, int courseId)
        {
            Student? student = _studentsDAO.GetStudentById(studentEmail);
            if (student == null || student.ActiveCourseId != -1) return;
            if (student.RequestedCourses.Contains(courseId))
            {
                student.RequestedCourses.Remove(courseId);
                student.ActiveCourseId = courseId;
                _studentsDAO.SaveChanges();
            }
        }
        public void AddCourseRequest(string studentEmail, int courseId)
        {
            Student? student = _studentsDAO.GetStudentById(studentEmail);
            if (student != null && !student.RequestedCourses.Contains(courseId))
            {
                student.RequestedCourses.Add(courseId);
                _studentsDAO.SaveChanges();
            }
        }
        public void RemoveCourseRequest(string studentEmail, int courseId)
        {
            Student? student = _studentsDAO.GetStudentById(studentEmail);
            if (student != null && student.RequestedCourses.Contains(courseId))
            {
                student.RequestedCourses.Remove(courseId);
                _studentsDAO.SaveChanges();
            }
        }
    }
}