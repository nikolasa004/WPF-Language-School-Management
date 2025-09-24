using System;
using System.Text.RegularExpressions;

namespace LangLang.DTO.Validators
{
    class StudentDTOValidator
    {
        private readonly Regex _nameRegex = new Regex("^[A-Za-z]+$");
        private readonly Regex _phoneRegex = new Regex(@"^[0-9\+]+$");
        private readonly Regex _emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        private readonly string[] _validatedProperties ={ "FirstName", "LastName", "DateOfBirth", "PhoneNumber", "Email", "Password" };
        public string Validate(StudentDTO student, string propertyName) => propertyName switch
        {
            nameof(StudentDTO.FirstName) =>string.IsNullOrWhiteSpace(student.FirstName) ? "Please enter your first name." :!_nameRegex.IsMatch(student.FirstName) ? "First name can only contain letters." :string.Empty,
            nameof(StudentDTO.LastName) =>string.IsNullOrWhiteSpace(student.LastName) ? "Please enter your last name." :!_nameRegex.IsMatch(student.LastName) ? "Last name can only contain letters." :string.Empty,
            nameof(StudentDTO.DateOfBirth) =>student.DateOfBirth == null ? "Please select your birth date." :!DateTime.TryParse(student.DateOfBirth, out var birthDate) ? "Invalid date format." :birthDate > DateTime.Now ? "Birth date cannot be in the future." :
            birthDate < DateTime.Now.AddYears(-120) ? "Birth date cannot be more than 120 years ago." : birthDate > DateTime.Now.AddYears(-18) ? "You must be at least 18 years old to register." :string.Empty,
            nameof(StudentDTO.PhoneNumber) =>string.IsNullOrWhiteSpace(student.PhoneNumber) ? "Please enter your phone number." :!_phoneRegex.IsMatch(student.PhoneNumber) ? "Phone number can only contain digits and '+' sign." :string.Empty,
            nameof(StudentDTO.Email) =>string.IsNullOrWhiteSpace(student.Email) ? "Please enter your email." :!_emailRegex.IsMatch(student.Email) ? "Please enter a valid email address." :string.Empty,
            nameof(StudentDTO.Password) =>string.IsNullOrWhiteSpace(student.Password) ? "Please enter your password." :string.Empty,

            _ => string.Empty
        };


        public bool IsValid(StudentDTO student)
        {
            foreach (var property in _validatedProperties)
            {
                if (Validate(student, property) != string.Empty)
                    return false;
            }
            return true;
        }
    }
}