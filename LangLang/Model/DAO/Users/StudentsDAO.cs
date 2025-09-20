using LangLang.Storage;
using System.Collections.Generic;
using LangLang.Observer;
using LangLang.Model.Users;

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
    }
}