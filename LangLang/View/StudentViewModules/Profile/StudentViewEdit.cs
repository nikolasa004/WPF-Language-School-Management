using LangLang.Controller.Users;
using LangLang.DTO;
using LangLang.Model.Enums;
using LangLang.Model.Users;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace LangLang.View.StudentViewModules.Profile
{
    class StudentViewEdit
    {
        private readonly StudentsController studentsController;

        public StudentDTO Student;
        public StudentViewProfile studentViewProfile;

        private TextBox FirstNameBox;
        private TextBox LastNameBox;
        private RadioButton MaleRadioButton;
        private RadioButton FemaleRadioButton;
        private DatePicker BirthDatePicker;
        private TextBox PhoneBox;
        private TextBox EmailBox;
        private TextBox PasswordTextBox;
        private ComboBox EducationComboBox;

        public StudentViewEdit(StudentDTO student, StudentsController studentsController, StudentViewProfile studentViewProfile, TextBox firstNameBox, TextBox lastNameBox, RadioButton maleRadioButton, RadioButton femaleRadioButton, DatePicker birthDatePicker, TextBox phoneBox, TextBox emailBox, TextBox passwordTextBox, ComboBox educationComboBox)
        {
            Student = student;
            this.studentsController = studentsController;
            this.studentViewProfile = studentViewProfile;
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
                studentViewProfile.InformationUpdate(true, Student);

                MessageBox.Show("Update was successful!", "Update successful", MessageBoxButton.OK, MessageBoxImage.Information);

                return true;
            }
            return false;
        }
    }
}