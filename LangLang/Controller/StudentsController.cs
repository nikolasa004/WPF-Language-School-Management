using System.Collections.Generic;
using LangLang.Model;
using LangLang.Model.DAO;
using LangLang.Observer;

namespace LangLang.Controller
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

        public void Subscribe(IObserver observer)
        {
            _students.StudentSubject.Subscribe(observer);
        }
    }
}
