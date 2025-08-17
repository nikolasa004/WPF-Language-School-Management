using LangLang.Controller;
using LangLang.DTO;
using LangLang.Model;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace LangLang.RoleWindows
{
    /// <summary>
    /// Interaction logic for StudentWindow.xaml
    /// </summary>
    public partial class StudentWindow : Window
    {
        public StudentDTO Student { get; set; }
        private StudentsController studentsController;

        public StudentWindow(Student student,StudentsController studentsController)
        {
            InitializeComponent();
            NameLabel.Content = student.FirstName;
            SurnameLabel.Content = student.LastName;
            GenderLabel.Content = student.Gender;
            DateTime birthDate = DateTime.ParseExact(student.DateOfBirth, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            string birthDateString = birthDate.ToString("dd.MM.yyyy");
            BirthDateLabel.Content = birthDateString;
            PhoneLabel.Content = student.PhoneNumber;
            EmailLabel.Content = student.Email;
            EducationLabel.Content = student.EducationLevel.ToString();
            CurrentCourseLabel.Content = "Not enrolled in any course";
            PenaltyPointsLabel.Content = student.PenaltyPoints.ToString();
            JoinedDateLabel.Content = student.DateOfRegistration;

            Student = new StudentDTO(student);
            this.studentsController = studentsController;
        }

        private void CoursesButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void ExamsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.IsOpen = true;
            }
        }
    }
}
