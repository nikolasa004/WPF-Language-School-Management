using LangLang.Model;
using LangLang.Model.Enums;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace LangLang.DTO
{
    public class StudentDTO : INotifyPropertyChanged, IDataErrorInfo
    {
        private string firstName;
        public string FirstName
        {
            get
            {
                return firstName;
            }
            set
            {
                if (value != firstName)
                {
                    firstName = value;
                    OnPropertyChanged("FirstName");
                }
            }
        }
        private string lastName;
        public string LastName
        {
            get
            {
                return lastName;
            }
            set
            {
                if (value != lastName)
                {
                    lastName = value;
                    OnPropertyChanged("LastName");
                }
            }
        }
        private Gender gender;
        public Gender Gender
        {
            get
            {
                return gender;
            }
            set
            {
                if (value!=gender)
                {
                    gender = value;
                    OnPropertyChanged("Gender");
                }
            }
        }
        private string dateOfBirth;
        public string DateOfBirth
        {
            get
            {
                return dateOfBirth;
            }
            set
            {
                if (value != dateOfBirth)
                {
                    dateOfBirth = value;
                    OnPropertyChanged("DateOfBirth");
                }
            }
        }
        private string phoneNumber;
        public string PhoneNumber
        {
            get
            {
                return phoneNumber;
            }
            set
            {
                if (value != phoneNumber)
                {
                    phoneNumber = value;
                    OnPropertyChanged("PhoneNumber");
                }
            }
        }
        public string Email { get; set; }
        private string password;
        public string Password
        {  
            get
            {
                return password;
            }
            set
            {
                if (value != password)
                {
                    password = value;
                    OnPropertyChanged("Password");
                }
            }
        }
        private EducationLevel educationLevel;
        public EducationLevel EducationLevel
        {
            get
            {
                return educationLevel;
            }
            set
            {
                if (value != educationLevel)
                {
                    educationLevel = value;
                    OnPropertyChanged("EducationLevel");
                }
            }
        }
        private string dateOfRegistration;
        public string DateOfRegistration
        {
            get
            {
                return dateOfRegistration;
            }
            set
            {
                if (value != dateOfRegistration)
                {
                    dateOfRegistration = value;
                    OnPropertyChanged("DateOfRegistration");
                }
            }
        }
        public int PenaltyPoints { get; set; }

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
                            return "Please enter your first name.";
                        if (!_nameRegex.IsMatch(FirstName))
                            return "First name can only contain letters.";
                        break;

                    case nameof(LastName):
                        if (string.IsNullOrWhiteSpace(LastName))
                            return "Please enter your last name.";
                        if (!_nameRegex.IsMatch(LastName))
                            return "Last name can only contain letters.";
                        break;

                    case nameof(DateOfBirth):
                        if (DateOfBirth == null)
                            return "Please select your birth date.";
                        if (!DateTime.TryParse(DateOfBirth, out var birthDate))
                            return "Invalid date format.";
                        if (birthDate > DateTime.Now)
                            return "Birth date cannot be in the future.";
                        if (birthDate < DateTime.Now.AddYears(-120))
                            return "Birth date cannot be more than 120 years ago.";
                        if (birthDate > DateTime.Now.AddYears(-18))
                            return "You must be at least 18 years old to register.";
                        break;

                    case nameof(PhoneNumber):
                        if (string.IsNullOrWhiteSpace(PhoneNumber))
                            return "Please enter your phone number.";
                        if (!_phoneRegex.IsMatch(PhoneNumber))
                            return "Phone number can only contain digits and '+' sign.";
                        break;

                    case nameof(Email):
                        if (string.IsNullOrWhiteSpace(Email))
                            return "Please enter your email.";
                        if (!_emailRegex.IsMatch(Email))
                            return "Please enter a valid email address.";
                        break;

                    case nameof(Password):
                        if (string.IsNullOrWhiteSpace(Password))
                            return "Please enter your password.";
                        break;
                }

                return string.Empty;
            }
        }

        private readonly string[] _validatedProperties = {"FirstName", "LastName", "DateOfBirth", "PhoneNumber", "Email", "Password"};

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

        public Student ToStudent()
        {
            return new Student(FirstName,LastName,Gender,DateOfBirth,PhoneNumber,Email,Password,EducationLevel,DateOfRegistration,PenaltyPoints);
        }

        public StudentDTO()
        {
            firstName = string.Empty;
            lastName = string.Empty;
            dateOfBirth = string.Empty;
            phoneNumber = string.Empty;
            Email = string.Empty;
            password = string.Empty;
            dateOfRegistration = string.Empty;
            PenaltyPoints = 0;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public StudentDTO(Student student)
        {
            firstName = student.FirstName;
            lastName = student.LastName;
            dateOfBirth = student.DateOfBirth;
            gender = student.Gender;
            phoneNumber = student.PhoneNumber;
            Email = student.Email;
            password = student.Password;
            educationLevel = student.EducationLevel;
            dateOfRegistration = student.DateOfRegistration;
            PenaltyPoints = student.PenaltyPoints;
        }

        protected virtual void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
