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
        public string Validate(StudentDTO student, string propertyName)
        {
            switch (propertyName)
            {
                case nameof(StudentDTO.FirstName):
                    if (string.IsNullOrWhiteSpace(student.FirstName))
                        return "Please enter your first name.";
                    if (!_nameRegex.IsMatch(student.FirstName))
                        return "First name can only contain letters.";
                    break;

                case nameof(StudentDTO.LastName):
                    if (string.IsNullOrWhiteSpace(student.LastName))
                        return "Please enter your last name.";
                    if (!_nameRegex.IsMatch(student.LastName))
                        return "Last name can only contain letters.";
                    break;

                case nameof(StudentDTO.DateOfBirth):
                    if (student.DateOfBirth == null)
                        return "Please select your birth date.";
                    if (!DateTime.TryParse(student.DateOfBirth, out var birthDate))
                        return "Invalid date format.";
                    if (birthDate > DateTime.Now)
                        return "Birth date cannot be in the future.";
                    if (birthDate < DateTime.Now.AddYears(-120))
                        return "Birth date cannot be more than 120 years ago.";
                    if (birthDate > DateTime.Now.AddYears(-18))
                        return "You must be at least 18 years old to register.";
                    break;

                case nameof(StudentDTO.PhoneNumber):
                    if (string.IsNullOrWhiteSpace(student.PhoneNumber))
                        return "Please enter your phone number.";
                    if (!_phoneRegex.IsMatch(student.PhoneNumber))
                        return "Phone number can only contain digits and '+' sign.";
                    break;

                case nameof(StudentDTO.Email):
                    if (string.IsNullOrWhiteSpace(student.Email))
                        return "Please enter your email.";
                    if (!_emailRegex.IsMatch(student.Email))
                        return "Please enter a valid email address.";
                    break;

                case nameof(StudentDTO.Password):
                    if (string.IsNullOrWhiteSpace(student.Password))
                        return "Please enter your password.";
                    break;
            }

            return string.Empty;
        }

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