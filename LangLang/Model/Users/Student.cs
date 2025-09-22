using LangLang.Model.Enums;
using LangLang.Storage.Serialization;
using System.Text.Json;
using System.Collections.Generic;
using System;

namespace LangLang.Model.Users
{
    public class Student : User, ISerializable
    {
        public int PenaltyPoints { get; set; }

        public string LastPenaltyProcessedAt { get; set; }
        public List<int> RequestedCourses { get; set; }
        public int ActiveCourseId { get; set; }
        public List<Tuple<Language, LanguageLevel>> FinishedCourses { get; set; }
        
        public List<int> RequestedExamTerms { get; set; }
        public int ActiveExamTermId { get; set; }
        public List<Tuple<Language, LanguageLevel>> FinishedExams { get; set; }

        public Student() : base()
        {
            PenaltyPoints = 0;
            RequestedCourses = new List<int>();
            ActiveCourseId = -1;
            FinishedCourses = new List<Tuple<Language, LanguageLevel>>();
            LastPenaltyProcessedAt = DateTime.Now.ToString("yyyy-MM-dd");
            RequestedExamTerms = new List<int>();
            ActiveExamTermId = -1;
            FinishedExams = new List<Tuple<Language, LanguageLevel>>();
        }
        public Student(string firstName, string lastName, Gender gender, string dateOfBirth, string phoneNumber, string email, string password, EducationLevel educationLevel, string dateOfRegistration, int penaltyPoints, List<int> requestedCourses, int activeCourseId, List<Tuple<Language, LanguageLevel>> finishedCourses, string lastPenaltyProcessedAt) : base(firstName, lastName, gender, dateOfBirth, phoneNumber, email, password, educationLevel, dateOfRegistration)
        {
            PenaltyPoints = penaltyPoints;
            RequestedCourses = requestedCourses;
            ActiveCourseId = activeCourseId;
            FinishedCourses = finishedCourses;
            LastPenaltyProcessedAt = lastPenaltyProcessedAt;

            RequestedExamTerms = new List<int>();
            ActiveExamTermId = -1;
            FinishedExams = new List<Tuple<Language, LanguageLevel>>();
        }

        public override string ToString()
        {
            return base.ToString() + "\n" + "Penalty Points: " + PenaltyPoints+ "\n" + "Requested Courses: " + string.Join(", ", RequestedCourses) + "\n" + "Active Course ID: " + ActiveCourseId + "\n" + "Finished Courses: " + string.Join(", ", FinishedCourses) + "\n" + "\nRequested Exam Terms: " + string.Join(", ", RequestedExamTerms)+ "\nActive Exam Term ID: " + ActiveExamTermId + "\nFinished Exams: " + string.Join(", ", FinishedExams) + "\n Last Penalty Processed At: " +  LastPenaltyProcessedAt;
        }

        public string ToJSON()
        {
            var options = new JsonSerializerOptions{WriteIndented = true};
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
                RequestedCourses = student.RequestedCourses;
                ActiveCourseId = student.ActiveCourseId;
                FinishedCourses = student.FinishedCourses;
                LastPenaltyProcessedAt = student.LastPenaltyProcessedAt;
                RequestedExamTerms = student.RequestedExamTerms ?? new List<int>();
                ActiveExamTermId = student.ActiveExamTermId;
                FinishedExams = student.FinishedExams ?? new List<Tuple<Language, LanguageLevel>>();
            }
        }
    }
}