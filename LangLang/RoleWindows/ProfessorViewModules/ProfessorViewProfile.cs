using LangLang.Controller;
using LangLang.DTO;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace LangLang.RoleWindows.ProfessorViewModules
{
    internal class ProfessorViewProfile
    {
        private readonly ProfessorDTO Professor;

        private Label NameLabel = null!;
        private Label SurnameLabel = null!;
        private Label GenderLabel = null!;
        private Label BirthDateLabel = null!;
        private Label PhoneLabel = null!;
        private Label EmailLabel = null!;
        private Label EducationLabel = null!;
        private TextBlock LanguagesLabel = null!;
        private Label JoinedDateLabel = null!;

        public ProfessorViewProfile(ProfessorDTO professor)
        {
            Professor = professor;
        }

        public void SetLabels(
            Label nameLabel,
            Label surnameLabel,
            Label genderLabel,
            Label birthDateLabel,
            Label phoneLabel,
            Label emailLabel,
            Label educationLabel,
            TextBlock languagesLabel,
            Label joinedDateLabel)
        {
            NameLabel = nameLabel;
            SurnameLabel = surnameLabel;
            GenderLabel = genderLabel;
            BirthDateLabel = birthDateLabel;
            PhoneLabel = phoneLabel;
            EmailLabel = emailLabel;
            EducationLabel = educationLabel;
            LanguagesLabel = languagesLabel;
            JoinedDateLabel = joinedDateLabel;
        }

        public void InformationUpdate()
        {
            NameLabel.Content = "Name: " + Professor.FirstName;
            SurnameLabel.Content = "Surname: " + Professor.LastName;
            GenderLabel.Content = "Gender: " + Professor.Gender;

            if (DateTime.TryParseExact(Professor.DateOfBirth, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime birthDate))
                BirthDateLabel.Content = "Birth Date: " + birthDate.ToString("dd.MM.yyyy", new CultureInfo("sr-RS"));
            else
                BirthDateLabel.Content = "Birth Date: " + Professor.DateOfBirth;

            PhoneLabel.Content = "Phone: " + Professor.PhoneNumber;
            EmailLabel.Content = "Email: " + Professor.Email;
            EducationLabel.Content = "Education: " + Professor.EducationLevel.ToString();

            if (Professor.Languages != null && Professor.Languages.Count > 0)
            {
                var languagesText = string.Join(", ", Professor.Languages.Select(kvp => $"{kvp.Key} - {kvp.Value}"));
                LanguagesLabel.Text = "Languages: " + languagesText;
            }
            else
            {
                LanguagesLabel.Text = "Languages: -";
            }

            if (DateTime.TryParseExact(Professor.DateOfRegistration, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime joinedDate))
                JoinedDateLabel.Content = "Joined Date: " + joinedDate.ToString("dd.MM.yyyy", new CultureInfo("sr-RS"));
            else
                JoinedDateLabel.Content = "Joined Date: " + Professor.DateOfRegistration;
        }
    }
}
