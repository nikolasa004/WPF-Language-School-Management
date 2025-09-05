using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace LangLang.DTO.Validators
{
    class ProfessorDTOValidator
    {
        private readonly Regex _nameRegex = new Regex("^[A-Za-z]+$");
        private readonly Regex _phoneRegex = new Regex(@"^[0-9\+]+$");
        private readonly Regex _emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        private readonly string[] _validatedProperties =
        {
            nameof(ProfessorDTO.FirstName),
            nameof(ProfessorDTO.LastName),
            nameof(ProfessorDTO.DateOfBirth),
            nameof(ProfessorDTO.PhoneNumber),
            nameof(ProfessorDTO.Email),
            nameof(ProfessorDTO.Password)
        };

        public string Validate(ProfessorDTO professor, string propertyName)
        {
            switch (propertyName)
            {
                case nameof(ProfessorDTO.FirstName):
                    if (string.IsNullOrWhiteSpace(professor.FirstName))
                        return "Please enter first name.";
                    if (!_nameRegex.IsMatch(professor.FirstName))
                        return "First name can only contain letters.";
                    break;

                case nameof(ProfessorDTO.LastName):
                    if (string.IsNullOrWhiteSpace(professor.LastName))
                        return "Please enter last name.";
                    if (!_nameRegex.IsMatch(professor.LastName))
                        return "Last name can only contain letters.";
                    break;

                case nameof(ProfessorDTO.DateOfBirth):
                    if (professor.DateOfBirth == null)
                        return "Please select birth date.";
                    if (!DateTime.TryParse(professor.DateOfBirth, out var birthDate))
                        return "Invalid date format.";
                    if (birthDate > DateTime.Now)
                        return "Birth date cannot be in the future.";
                    if (birthDate < DateTime.Now.AddYears(-120))
                        return "Birth date cannot be more than 120 years ago.";
                    break;

                case nameof(ProfessorDTO.PhoneNumber):
                    if (string.IsNullOrWhiteSpace(professor.PhoneNumber))
                        return "Please enter phone number.";
                    if (!_phoneRegex.IsMatch(professor.PhoneNumber))
                        return "Phone number can only contain digits and '+' sign.";
                    break;

                case nameof(ProfessorDTO.Email):
                    if (string.IsNullOrWhiteSpace(professor.Email))
                        return "Please enter email.";
                    if (!_emailRegex.IsMatch(professor.Email))
                        return "Please enter a valid email address.";
                    break;

                case nameof(ProfessorDTO.Password):
                    if (string.IsNullOrWhiteSpace(professor.Password))
                        return "Please enter password.";
                    break;
            }

            return string.Empty;
        }

        public bool IsValid(ProfessorDTO professor)
        {
            foreach (var property in _validatedProperties)
            {
                if (Validate(professor, property) != string.Empty)
                {
                    Debug.WriteLine($"Validation failed for {property}: {Validate(professor, property)}");
                    return false;
                }
            }
            return true;
        }
    }
}
