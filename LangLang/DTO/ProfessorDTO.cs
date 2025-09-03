using LangLang.Model;
using LangLang.Model.Enums;
using LangLang.DTO.Validators;
using System.Collections.Generic;
using System.ComponentModel;

namespace LangLang.DTO
{
    public class ProfessorDTO : INotifyPropertyChanged, IDataErrorInfo
    {
        private bool SetProperty<T>(ref T storage, T value, string propertyName)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private string firstName = string.Empty;
        public string FirstName
        {
            get => firstName;
            set => SetProperty(ref firstName, value, nameof(FirstName));
        }

        private string lastName = string.Empty;
        public string LastName
        {
            get => lastName;
            set => SetProperty(ref lastName, value, nameof(LastName));
        }

        private Gender gender;
        public Gender Gender
        {
            get => gender;
            set => SetProperty(ref gender, value, nameof(Gender));
        }

        private string dateOfBirth = string.Empty;
        public string DateOfBirth
        {
            get => dateOfBirth;
            set => SetProperty(ref dateOfBirth, value, nameof(DateOfBirth));
        }

        private string phoneNumber = string.Empty;
        public string PhoneNumber
        {
            get => phoneNumber;
            set => SetProperty(ref phoneNumber, value, nameof(PhoneNumber));
        }

        public string Email { get; set; } = string.Empty;

        private string password = string.Empty;
        public string Password
        {
            get => password;
            set => SetProperty(ref password, value, nameof(Password));
        }

        private EducationLevel educationLevel = EducationLevel.Bachelor;
        public EducationLevel EducationLevel
        {
            get => educationLevel;
            set => SetProperty(ref educationLevel, value, nameof(EducationLevel));
        }

        private string dateOfRegistration = string.Empty;
        public string DateOfRegistration
        {
            get => dateOfRegistration;
            set => SetProperty(ref dateOfRegistration, value, nameof(DateOfRegistration));
        }

        private Dictionary<Language, LanguageLevel> languages = new();
        public Dictionary<Language, LanguageLevel> Languages
        {
            get => languages;
            set => SetProperty(ref languages, value, nameof(Languages));
        }

        public ProfessorDTO() { }

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

        // Validacija delegirana na validator klasu
        private readonly ProfessorDTOValidator validator = new ProfessorDTOValidator();
        public string this[string columnName] => validator.Validate(this, columnName);
        public bool IsValid => validator.IsValid(this);

        public string Error => string.Empty;
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

