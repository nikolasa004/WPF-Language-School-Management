using LangLang.Controller;
using LangLang.DTO;
using LangLang.Model;
using LangLang.Model.Enums;
using LangLang.RoleWindows.StudentViewModules;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LangLang.RoleWindows
{
    /// <summary>
    /// Interaction logic for StudentWindow.xaml
    /// </summary>
    public partial class StudentWindow : Window
    {
        public StudentDTO Student { get; set; }
        public StudentDTO UpdateStudent { get; set; }
        private StudentsController studentsController;

        private StudentViewExams studentViewExams;
        private StudentViewCourses studentViewCourses;

        public StudentWindow(Student student,StudentsController studentsController)
        {
            InitializeComponent();
            DataContext = this;

            Student = new StudentDTO(student);
            UpdateStudent = new StudentDTO(student);
            this.studentsController = studentsController;

            InformationUpdate();

            studentViewExams = new StudentViewExams(ExamLanguageCombo, ExamLevelCombo, ExamFromDate, ExamToDate, ExamsGrid);
            studentViewCourses = new StudentViewCourses(CourseLanguageCombo, CourseLevelCombo, CourseOnlineCombo, CourseWeeksBox, CourseFromDate, CourseToDate, CoursesGrid);
        }

        private void InformationUpdate()
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
            CurrentCourseLabel.Content = "Current Course: " + "/";
            PenaltyPointsLabel.Content = "Penalty Points: " + Student.PenaltyPoints.ToString();
            DateTime joinedDate = DateTime.ParseExact(Student.DateOfRegistration, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            string joinedDateString = joinedDate.ToString("dd.MM.yyyy");
            JoinedDateLabel.Content = "Joined Date: " + joinedDateString;
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

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            FirstNameBox.Text = Student.FirstName;
            LastNameBox.Text = Student.LastName;
            if (Student.Gender==Gender.Male)
            {
                MaleRadioButton.IsChecked = true;
            }
            else
            {
                FemaleRadioButton.IsChecked = true;
            }
            BirthDatePicker.SelectedDate = DateTime.ParseExact(Student.DateOfBirth, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            PhoneBox.Text = Student.PhoneNumber;
            EmailBox.Text = Student.Email;
            PasswordTextBox.Text = Student.Password;
            EducationComboBox.SelectedItem = Student.EducationLevel;

            MainPanel.Visibility = Visibility.Collapsed;
            EditPanel.Visibility = Visibility.Visible;
        }

        private void SaveEditChanges_Click(object sender, RoutedEventArgs e)
        {
            if (UpdateStudent.IsValid)
            {
                Student student = UpdateStudent.ToStudent();
                Student = new StudentDTO(student);

                studentsController.Update(student);

                InformationUpdate();

                MessageBox.Show("Update was successful!", "Update successful", MessageBoxButton.OK, MessageBoxImage.Information);

                CloseEditPanel_Click(sender, e);
            }
        }

        private void CloseEditPanel_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Visibility = Visibility.Visible;
            EditPanel.Visibility = Visibility.Collapsed;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to delete your account? All your data will be lost.",
                "Delete Account",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                studentsController.Delete(Student.Email);
                MessageBox.Show("Your account has been deleted successfully.");
                Close();
            }
        }

        // Exam Terms Panel
        private void ShowAllExams_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Visibility = Visibility.Collapsed;
            ExamsPanel.Visibility = Visibility.Visible;
        }

        private void CloseExamsPanel_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Visibility = Visibility.Visible;
            ExamsPanel.Visibility = Visibility.Collapsed;
        }

        private void ExamsSearch_Click(object sender, RoutedEventArgs e)
        {
            studentViewExams.SearchExams(sender, e);
        }

        private void PrevPageExams_Click(object sender, RoutedEventArgs e)
        {
            studentViewExams.PrevPage(sender, e);
        }

        private void NextPageExams_Click(object sender, RoutedEventArgs e)
        {
            studentViewExams.NextPage(sender, e);
        }

        private void PageSizeComboExams_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(studentViewExams==null) return;
            studentViewExams.PageSizeChanged(sender, e);
        }

        private void ExamsGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            studentViewExams.ExamsGridSorting(sender, e);
        }

        // Course Terms Panel
        private void ShowAllCourses_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Visibility = Visibility.Collapsed;
            CoursesPanel.Visibility = Visibility.Visible;
        }

        private void CloseCoursesPanel_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Visibility = Visibility.Visible;
            CoursesPanel.Visibility = Visibility.Collapsed;
        }

        private void CoursesSearch_Click(object sender, RoutedEventArgs e)
        {
            studentViewCourses.SearchCourses(sender, e);
        }

        private void PrevPageCourses_Click(object sender, RoutedEventArgs e)
        {
            studentViewCourses.PrevPage(sender, e);
        }

        private void NextPageCourses_Click(object sender, RoutedEventArgs e)
        {
            studentViewCourses.NextPage(sender, e);
        }

        private void PageSizeComboCourses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (studentViewCourses == null) return;
            studentViewCourses.PageSizeChanged(sender, e);
        }

        private void CoursesGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            studentViewCourses.CoursesGridSorting(sender, e);
        }
    }
}
