using LangLang.Model.Enums;
using LangLang.Storage.Serialization;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace LangLang.Model.Users
{
    public class Professor : User, ISerializable
    {
        // Jezici koje profesor predaje i njihovi nivoi
        public Dictionary<Language, LanguageLevel> Languages { get; set; }
        public Tuple<int, int> AverageRating { get; set; }

        public Professor() : base()
        {
            Languages = new Dictionary<Language, LanguageLevel>();
            AverageRating = new Tuple<int, int>(0, 0);
        }

        public Professor(string firstName, string lastName, Gender gender, string dateOfBirth, string phoneNumber, string email, string password, EducationLevel educationLevel, string dateOfRegistration, Dictionary<Language, LanguageLevel> languages, Tuple<int, int> averageRating)
            : base(firstName, lastName, gender, dateOfBirth, phoneNumber, email, password, educationLevel, dateOfRegistration)
        {
            Languages = languages ?? new Dictionary<Language, LanguageLevel>();
            AverageRating = averageRating;
        }

        public override string ToString()
        {
            return base.ToString() + "\nJezici: " + string.Join(", ", Languages)+ "\nProsečna ocena: " + (AverageRating.Item2 == 0 ? "N/A" : (AverageRating.Item1 / AverageRating.Item2).ToString("F2"));
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
                AverageRating = professor.AverageRating;
            }
        }
    }
}