using LangLang.Model.Enums;
using LangLang.Storage.Serialization;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace LangLang.Model
{
    public class Professor : User, ISerializable
    {
        // Jezici koje profesor predaje i njihovi nivoi
        public Dictionary<Language, LanguageLevel> Languages { get; set; }

        public Professor() : base()
        {
            Languages = new Dictionary<Language, LanguageLevel>();
        }

        public Professor(string firstName, string lastName, Gender gender, string dateOfBirth, string phoneNumber, string email, string password, EducationLevel educationLevel, string dateOfRegistration, Dictionary<Language, LanguageLevel> languages)
            : base(firstName, lastName, gender, dateOfBirth, phoneNumber, email, password, educationLevel, dateOfRegistration)
        {
            Languages = languages ?? new Dictionary<Language, LanguageLevel>();
        }

        public override string ToString()
        {
            return base.ToString() + "\nJezici: " + string.Join(", ", Languages);
        }

        public string ToJSON()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            return JsonSerializer.Serialize(this, options);
        }

        public void FromJSON(string value)
        {
            Professor? professor = JsonSerializer.Deserialize<Professor>(value);
            if (professor != null)
            {
                FirstName = professor.FirstName;
                LastName = professor.LastName;
                Gender = professor.Gender;
                DateOfBirth = professor.DateOfBirth;
                PhoneNumber = professor.PhoneNumber;
                Email = professor.Email;
                Password = professor.Password;
                EducationLevel = professor.EducationLevel;
                DateOfRegistration = professor.DateOfRegistration;
                Languages = professor.Languages ?? new Dictionary<Language, LanguageLevel>();
            }
        }
    }
}
