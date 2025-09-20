using LangLang.Controller.TeachingActivities;
using LangLang.Controller.Users;
using LangLang.DTO;
using LangLang.Model.Enums;
using LangLang.Model.TeachingActivities;
using LangLang.Model.Users;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace LangLang.RoleWindows.StudentViewModules
{
    class StudentViewProfile
    {
        private readonly StudentsController studentsController;
        private readonly CourseController courseController;
        private StudentDTO Student;

        private TextBox FirstNameBox;
        private TextBox LastNameBox;
        private RadioButton MaleRadioButton;
        private RadioButton FemaleRadioButton;
        private DatePicker BirthDatePicker;
        private TextBox PhoneBox;
        private TextBox EmailBox;
        private TextBox PasswordTextBox;
        private ComboBox EducationComboBox;

        private Label NameLabel = null!;
        private Label SurnameLabel = null!;
        private Label GenderLabel = null!;
        private Label BirthDateLabel = null!;
        private Label PhoneLabel = null!;
        private Label EmailLabel = null!;
        private Label EducationLabel = null!;
        private Label CurrentCourseLabel = null!;
        private Label PenaltyPointsLabel = null!;
        private Label JoinedDateLabel = null!;

        public StudentViewProfile(StudentDTO student, StudentsController studentsController, CourseController courseController, TextBox firstNameBox, TextBox lastNameBox, RadioButton maleRadioButton, RadioButton femaleRadioButton, DatePicker birthDatePicker, TextBox phoneBox, TextBox emailBox, TextBox passwordTextBox, ComboBox educationComboBox)
        {
            Student = student;
            this.studentsController = studentsController;
            this.courseController = courseController;
            FirstNameBox = firstNameBox;
            LastNameBox = lastNameBox;
            MaleRadioButton = maleRadioButton;
            FemaleRadioButton = femaleRadioButton;
            BirthDatePicker = birthDatePicker;
            PhoneBox = phoneBox;
            EmailBox = emailBox;
            PasswordTextBox = passwordTextBox;
            EducationComboBox = educationComboBox;
        }
        public void SetLabels(Label nameLabel, Label surnameLabel, Label genderLabel, Label birthDateLabel, Label phoneLabel, Label emailLabel, Label educationLabel, Label currentCourseLabel, Label penaltyPointsLabel, Label joinedDateLabel)
        {
            NameLabel = nameLabel;
            SurnameLabel = surnameLabel;
            GenderLabel = genderLabel;
            BirthDateLabel = birthDateLabel;
            PhoneLabel = phoneLabel;
            EmailLabel = emailLabel;
            EducationLabel = educationLabel;
            CurrentCourseLabel = currentCourseLabel;
            PenaltyPointsLabel = penaltyPointsLabel;
            JoinedDateLabel = joinedDateLabel;
        }

        public void InformationUpdate()
        {
            NameLabel.Content = "Name: " + Student.FirstName;
            SurnameLabel.Content = "Surname: " + Student.LastName;
            GenderLabel.Content = "Gender: " + Student.Gender;
            DateTime birthDate = DateTime.ParseExact(Student.DateOfBirth, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            string birthDateString = birthDate.ToString("dd.MM.yyyy");
            BirthDateLabel.Content = "Birth Date: " + birthDateString;
            PhoneLabel.Content = "Phone: " + Student.PhoneNumber;
            EmailLabel.Content = "Email: " + Student.Email;
            EducationLabel.Content = "Education: " + Student.EducationLevel.ToString();
            Course? course = courseController.GetById(Student.ActiveCourseId);
            string courseName = course != null ? course.Language+"-"+course.Level : "No active course";
            CurrentCourseLabel.Content = "Current Course: " + courseName;
            PenaltyPointsLabel.Content = "Penalty Points: " + Student.PenaltyPoints.ToString();
            DateTime joinedDate = DateTime.ParseExact(Student.DateOfRegistration, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            string joinedDateString = joinedDate.ToString("dd.MM.yyyy");
            JoinedDateLabel.Content = "Joined Date: " + joinedDateString;
        }

        public void SetStudentData()
        {
            FirstNameBox.Text = Student.FirstName;
            LastNameBox.Text = Student.LastName;
            if (Student.Gender == Gender.Male)
                MaleRadioButton.IsChecked = true;
            else
                FemaleRadioButton.IsChecked = true;
            BirthDatePicker.SelectedDate = DateTime.ParseExact(Student.DateOfBirth, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            PhoneBox.Text = Student.PhoneNumber;
            EmailBox.Text = Student.Email;
            PasswordTextBox.Text = Student.Password;
            EducationComboBox.SelectedItem = Student.EducationLevel;
        }
        public bool SaveEditChanges(StudentDTO UpdateStudent)
        {
            if (UpdateStudent.IsValid)
            {
                Student student = UpdateStudent.ToStudent();
                Student = new StudentDTO(student);

                studentsController.Update(student);

                InformationUpdate();

                MessageBox.Show("Update was successful!", "Update successful", MessageBoxButton.OK, MessageBoxImage.Information);

                return true;
            }
            return false;
        }
        public bool DeleteProfile()
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete your account? All your data will be lost.", "Delete Account", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                studentsController.Delete(Student.Email);
                MessageBox.Show("Your account has been deleted successfully.");
                return true;
            }
            return false;
        }
    }
}