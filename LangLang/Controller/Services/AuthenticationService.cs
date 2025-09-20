using LangLang.Controller.Users;
using LangLang.DTO;
using LangLang.Model.Users;
using System;

namespace LangLang.Controller.Services
{
    class AuthenticationService
    {
        private readonly StudentsController studentsController;
        private readonly ProfessorsController professorsController;
        private readonly string _principalEmail;
        private readonly string _principalPassword;

        public AuthenticationService(StudentsController studentsController,ProfessorsController professorsController,string principalEmail,string principalPassword)
        {
            this.studentsController = studentsController;
            this.professorsController = professorsController;
            _principalEmail = principalEmail;
            _principalPassword = principalPassword;
        }

        public object? Authenticate(string email, string password)
        {
            var student = studentsController.GetStudentById(email);
            if (IsValidStudent(student,password))
                return student;

            var professor = professorsController.GetProfessorById(email);
            if (IsValidProfessor(professor,password))
                return professor;

            if (IsPrincipalLogin(email,password))
                return "Principal";

            return null;
        }
        private bool IsValidStudent(Student? student, string password) =>
          student != null && student.Password == password;
        private bool IsValidProfessor(Professor? professor, string password) =>
            professor != null && professor.Password == password;
        private bool IsPrincipalLogin(string email, string password) =>
            email == _principalEmail && password == _principalPassword;

        public bool Register(StudentDTO studentDto, out string message)
        {
            if (!studentDto.IsValid)
            {
                message = "Invalid registration data.";
                return false;
            }

            if (studentsController.GetStudentById(studentDto.Email) != null)
            {
                message = "An account with this email already exists.";
                return false;
            }

            var student = studentDto.ToStudent();
            student.DateOfRegistration = DateTime.Now.ToString("yyyy-MM-dd");
            studentsController.Add(student);

            message = "Registration successful!";
            return true;
        }
    }
}