using LangLang.Model.Enums;
using LangLang.Storage.Serialization;
using System.Text.Json;

namespace LangLang.Model
{
    public class Student : User, ISerializable
    {
        public int PenaltyPoints { get; set; }
        // Here should be a list of requested courses
        // and a list of requested exams
        // and a list of finished courses
        // and a list of finished exams
        // and active course
        // and active exam

        public Student() : base()
        {
            PenaltyPoints = 0;
        }

        public Student(string firstName, string lastName, Gender gender, string dateOfBirth, string phoneNumber, string email, string password, EducationLevel educationLevel, string dateOfRegistration, int penaltyPoints) : base(firstName, lastName, gender, dateOfBirth, phoneNumber, email, password, educationLevel, dateOfRegistration)
        {
            PenaltyPoints = penaltyPoints;
        }

        public override string ToString()
        {
            return base.ToString() + "\n" + "Penalty Points: " + PenaltyPoints;
        }

        public string ToJSON()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            return JsonSerializer.Serialize(this,options);
        }

        public void FromJSON(string value)
        {
            Student? student = JsonSerializer.Deserialize<Student>(value);
            if (student != null)
            {
                FirstName = student.FirstName;
                LastName = student.LastName;
                Gender = student.Gender;
                DateOfBirth = student.DateOfBirth;
                PhoneNumber = student.PhoneNumber;
                Email = student.Email;
                Password = student.Password;
                EducationLevel = student.EducationLevel;
                DateOfRegistration = student.DateOfRegistration;
                PenaltyPoints = student.PenaltyPoints;
            }
        }
    }
}
