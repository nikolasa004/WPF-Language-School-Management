using LangLang.Model.Enums;

namespace LangLang.Model
{
    public class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Gender Gender { get; set; }
        public string DateOfBirth { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public EducationLevel EducationLevel { get; set; }
        public string DateOfRegistration { get; set; }

        public User()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            DateOfBirth = string.Empty;
            PhoneNumber = string.Empty;
            Email = string.Empty;
            Password = string.Empty;
            DateOfRegistration = string.Empty;
        }

        public User(string firstName, string lastName, Gender gender, string dateOfBirth, string phoneNumber, string email, string password, EducationLevel educationLevel, string dateOfRegistration)
        {
            FirstName = firstName;
            LastName = lastName;
            Gender = gender;
            DateOfBirth = dateOfBirth;
            PhoneNumber = phoneNumber;
            Email = email;
            Password = password;
            EducationLevel = educationLevel;
            DateOfRegistration = dateOfRegistration;
        }

        public override string ToString()
        {
            return "Name: " + FirstName + " " + LastName + "\n" + "Gender: " + Gender + "\n" +
                   "Date of Birth: " + DateOfBirth + "\n" + "Phone Number: " + PhoneNumber + "\n" +
                   "Email: " + Email + "\n" + "Education Level: " + EducationLevel + "\n" +
                   "Date of Registration: " + DateOfRegistration;
        }
    }
}
