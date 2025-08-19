using LangLang.Model;
using LangLang.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace LangLang.DTO
{
    public class ProfessorDTO : INotifyPropertyChanged, IDataErrorInfo
    {
        private string firstName;
        public string FirstName
        {
            get => firstName;
            set
            {
                if (value != firstName)
                {
                    firstName = value;
                    OnPropertyChanged(nameof(FirstName));
                }
            }
        }

        private string lastName;
        public string LastName
        {
            get => lastName;
            set
            {
                if (value != lastName)
                {
                    lastName = value;
                    OnPropertyChanged(nameof(LastName));
                }
            }
        }

        private Gender gender;
        public Gender Gender
        {
            get => gender;
            set
            {
                if (value != gender)
                {
                    gender = value;
                    OnPropertyChanged(nameof(Gender));
                }
            }
        }

        private string dateOfBirth;
        public string DateOfBirth
        {
            get => dateOfBirth;
            set
            {
                if (value != dateOfBirth)
                {
                    dateOfBirth = value;
                    OnPropertyChanged(nameof(DateOfBirth));
                }
            }
        }

        private string phoneNumber;
        public string PhoneNumber
        {
            get => phoneNumber;
            set
            {
                if (value != phoneNumber)
                {
                    phoneNumber = value;
                    OnPropertyChanged(nameof(PhoneNumber));
                }
            }
        }

        public string Email { get; set; }

        private string password;
        public string Password
        {
            get => password;
            set
            {
                if (value != password)
                {
                    password = value;
                    OnPropertyChanged(nameof(Password));
                }
            }
        }

        private EducationLevel educationLevel;
        public EducationLevel EducationLevel
        {
            get => educationLevel;
            set
            {
                if (value != educationLevel)
                {
                    educationLevel = value;
                    OnPropertyChanged(nameof(EducationLevel));
                }
            }
        }

        private string dateOfRegistration;
        public string DateOfRegistration
        {
            get => dateOfRegistration;
            set
            {
                if (value != dateOfRegistration)
                {
                    dateOfRegistration = value;
                    OnPropertyChanged(nameof(DateOfRegistration));
                }
            }
        }

        private Dictionary<Language, LanguageLevel> languages;
        public Dictionary<Language, LanguageLevel> Languages
        {
            get => languages;
            set
            {
                if (value != languages)
                {
                    languages = value;
                    OnPropertyChanged(nameof(Languages));
                }
            }
        }

        public string Error => string.Empty;

        private readonly Regex _nameRegex = new Regex("^[A-Za-z]+$");
        private readonly Regex _phoneRegex = new Regex(@"^[0-9\+]+$");
        private readonly Regex _emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(FirstName):
                        if (string.IsNullOrWhiteSpace(FirstName))
                            return "Please enter first name.";
                        if (!_nameRegex.IsMatch(FirstName))
                            return "First name can only contain letters.";
                        break;

                    case nameof(LastName):
                        if (string.IsNullOrWhiteSpace(LastName))
                            return "Please enter last name.";
                        if (!_nameRegex.IsMatch(LastName))
                            return "Last name can only contain letters.";
                        break;

                    case nameof(DateOfBirth):
                        if (DateOfBirth == null)
                            return "Please select birth date.";
                        if (!DateTime.TryParse(DateOfBirth, out var birthDate))
                            return "Invalid date format.";
                        if (birthDate > DateTime.Now)
                            return "Birth date cannot be in the future.";
                        if (birthDate < DateTime.Now.AddYears(-120))
                            return "Birth date cannot be more than 120 years ago.";
                        break;

                    case nameof(PhoneNumber):
                        if (string.IsNullOrWhiteSpace(PhoneNumber))
                            return "Please enter phone number.";
                        if (!_phoneRegex.IsMatch(PhoneNumber))
                            return "Phone number can only contain digits and '+' sign.";
                        break;

                    case nameof(Email):
                        if (string.IsNullOrWhiteSpace(Email))
                            return "Please enter email.";
                        if (!_emailRegex.IsMatch(Email))
                            return "Please enter a valid email address.";
                        break;

                    case nameof(Password):
                        if (string.IsNullOrWhiteSpace(Password))
                            return "Please enter password.";
                        break;
                }

                return string.Empty;
            }
        }

        private readonly string[] _validatedProperties = { "FirstName", "LastName", "DateOfBirth", "PhoneNumber", "Email", "Password" };

        public bool IsValid
        {
            get
            {
                foreach (var property in _validatedProperties)
                {
                    if (this[property] != string.Empty)
                    {
                        Debug.WriteLine($"Validation failed for {property}: {this[property]}");
                        return false;
                    }
                }
                return true;
            }
        }

        public Professor ToProfessor()
        {
            return new Professor(
                FirstName,
                LastName,
                Gender,
                DateOfBirth,
                PhoneNumber,
                Email,
                Password,
                EducationLevel,
                DateOfRegistration,
                Languages
           
            );
        }

        public ProfessorDTO()
        {
            firstName = string.Empty;
            lastName = string.Empty;
            dateOfBirth = string.Empty;
            phoneNumber = string.Empty;
            Email = string.Empty;
            password = string.Empty;
            dateOfRegistration = string.Empty;
            educationLevel = EducationLevel.Bachelor;
            languages = new Dictionary<Language, LanguageLevel>();

        }

        public ProfessorDTO(Professor professor)
        {
            firstName = professor.FirstName;
            lastName = professor.LastName;
            gender = professor.Gender;
            dateOfBirth = professor.DateOfBirth;
            phoneNumber = professor.PhoneNumber;
            Email = professor.Email;
            password = professor.Password;
            dateOfRegistration = professor.DateOfRegistration;
            educationLevel = professor.EducationLevel;
            languages = new Dictionary<Language, LanguageLevel>(professor.Languages);

        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
