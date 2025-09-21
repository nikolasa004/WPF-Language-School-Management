using LangLang.Storage;
using System.Collections.Generic;
using LangLang.Observer;
using LangLang.Model.Users;
using LangLang.Model.Enums;
using System;

namespace LangLang.Model.DAO.Users
{
    class StudentsDAO
    {
        private readonly List<Student> _students;
        private readonly Storage<Student> _storage;

        public Subject StudentSubject;

        public StudentsDAO()
        {
            _storage = new Storage<Student>("students.json");
            _students = _storage.Load();
            StudentSubject = new Subject();
        }

        public Student AddStudent(Student student)
        {
            _students.Add(student);
            _storage.Save(_students);
            StudentSubject.NotifyObservers();
            return student;
        }
        public Student? UpdateStudent(Student student)
        {
            Student? oldStudent = GetStudentById(student.Email);
            if (oldStudent is null) return null;

            oldStudent.FirstName = student.FirstName;
            oldStudent.LastName = student.LastName;
            oldStudent.Gender = student.Gender;
            oldStudent.DateOfBirth = student.DateOfBirth;
            oldStudent.PhoneNumber = student.PhoneNumber;
            oldStudent.Password = student.Password;
            oldStudent.EducationLevel = student.EducationLevel;
            oldStudent.DateOfRegistration = student.DateOfRegistration;
            oldStudent.PenaltyPoints = student.PenaltyPoints;
            oldStudent.RequestedCourses = student.RequestedCourses;
            oldStudent.ActiveCourseId = student.ActiveCourseId;
            oldStudent.FinishedCourses = student.FinishedCourses;

            _storage.Save(_students);
            StudentSubject.NotifyObservers();
            return oldStudent;
        }
        public Student? RemoveStudent(string email)
        {
            Student? student = GetStudentById(email);
            if (student == null) return null;

            _students.Remove(student);
            _storage.Save(_students);
            StudentSubject.NotifyObservers();
            return student;
        }

        public Student? GetStudentById(string email)
        {
            return _students.Find(s => s.Email == email);
        }
        public List<Student> GetAllStudents()
        {
            return _students;
        }
        public List<int> GetRequestedCourses(string email)
        {
            Student? student = GetStudentById(email);
            if (student == null) return new List<int>();
            return student.RequestedCourses;
        }
        public List<Tuple<Language, LanguageLevel>> GetFinishedCourses(string email)
        {
            Student? student = GetStudentById(email);
            if (student == null) return new List<Tuple<Language, LanguageLevel>>();
            return student.FinishedCourses;
        }

        public void FinishActiveCourse(string studentEmail, Language language, LanguageLevel level)
        {
            Student? student = GetStudentById(studentEmail);
            if (student != null && student.ActiveCourseId != -1)
            {
                student.FinishedCourses.Add(new Tuple<Language, LanguageLevel>(language, level));
                student.ActiveCourseId = -1;
                _storage.Save(_students);
                StudentSubject.NotifyObservers();
            }
        }
    }
}