using LangLang.Model.Enums;
using LangLang.DTO.Validators;
using System.ComponentModel;
using LangLang.Model.Users;
using System.Collections.Generic;

namespace LangLang.DTO
{
    public class StudentDTO : INotifyPropertyChanged, IDataErrorInfo
    {
        private bool SetProperty<T>(ref T storage, T value, string propertyName)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private string firstName=string.Empty;
        public string FirstName
        {
            get => firstName;
            set => SetProperty(ref firstName, value, nameof(FirstName));
        }
        private string lastName=string.Empty;
        public string LastName
        {
            get => lastName;
            set => SetProperty(ref lastName, value, nameof(LastName));
        }
        private Gender gender;
        public Gender Gender
        {
            get=>gender;
            set=> SetProperty(ref gender, value, nameof(Gender));
        }
        private string dateOfBirth=string.Empty;
        public string DateOfBirth
        {
            get=> dateOfBirth;
            set=> SetProperty(ref dateOfBirth, value, nameof(DateOfBirth));
        }
        private string phoneNumber=string.Empty;
        public string PhoneNumber
        {
            get=> phoneNumber;
            set=> SetProperty(ref phoneNumber, value, nameof(PhoneNumber));
        }
        public string Email { get; set; }=string.Empty;
        private string password=string.Empty;
        public string Password
        {  
            get=> password;
            set=> SetProperty(ref password, value, nameof(Password));
        }
        private EducationLevel educationLevel;
        public EducationLevel EducationLevel
        {
            get=> educationLevel;
            set=> SetProperty(ref educationLevel, value, nameof(EducationLevel));
        }
        private string dateOfRegistration=string.Empty;
        public string DateOfRegistration
        {
            get=> dateOfRegistration;
            set=> SetProperty(ref dateOfRegistration, value, nameof(DateOfRegistration));
        }
        public int PenaltyPoints { get; set; }=0;
        public List<int> RequestedCourses { get; set; }=new List<int>();
        public int ActiveCourseId { get; set; }=-1;

        public StudentDTO(){}
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
            RequestedCourses = student.RequestedCourses;
            ActiveCourseId = student.ActiveCourseId;
        }
        public Student ToStudent()
        {
            return new Student(FirstName,LastName,Gender,DateOfBirth,PhoneNumber,Email,Password,EducationLevel,DateOfRegistration,PenaltyPoints, RequestedCourses,ActiveCourseId);
        }

        private readonly StudentDTOValidator validator = new StudentDTOValidator();
        public string this[string columnName] => validator.Validate(this, columnName);
        public bool IsValid => validator.IsValid(this);

        public string Error => string.Empty;
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}