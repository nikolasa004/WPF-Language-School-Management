using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LangLang.Controller;
using LangLang.DTO;
using LangLang.Model;
using LangLang.Model.DAO;

namespace LangLang.RoleWindows
{
    /// <summary>
    /// Interaction logic for ProfessorWindow.xaml
    /// </summary>
    public partial class ProfessorWindow : Window
    {
        public ProfessorDTO Professor { get; set; }
        public ProfessorDTO UpdateProfessor { get; set; }
        private ProfessorsController professorsController;

        public ProfessorWindow(Professor professor, ProfessorsController professorsController)
        {
            InitializeComponent();
            DataContext = this;

            Professor = new ProfessorDTO(professor);
            UpdateProfessor = new ProfessorDTO(professor);
            this.professorsController = professorsController;

            InformationUpdate();
        }

        private void InformationUpdate()
        {
            NameLabel.Content = "Name: " + Professor.FirstName;
            SurnameLabel.Content = "Surname: " + Professor.LastName;
            GenderLabel.Content = "Gender: " + Professor.Gender;

            // Formatiranje datuma rođenja
            if (DateTime.TryParseExact(Professor.DateOfBirth, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime birthDate))
            {
                BirthDateLabel.Content = "Birth Date: " + birthDate.ToString("dd.MM.yyyy");
            }
            else
            {
                BirthDateLabel.Content = "Birth Date: " + Professor.DateOfBirth;
            }

            PhoneLabel.Content = "Phone: " + Professor.PhoneNumber;
            EmailLabel.Content = "Email: " + Professor.Email;
            EducationLabel.Content = "Education: " + Professor.EducationLevel.ToString();

            // Languages
            if (Professor.Languages != null && Professor.Languages.Count > 0)
            {
                // Spoji svaki jezik sa njegovim nivoom, npr. "English - Advanced"
                var languagesText = string.Join(", ", Professor.Languages.Select(kvp => $"{kvp.Key} - {kvp.Value}"));
                LanguagesLabel.Text = "Languages: " + languagesText;
            }
            else
            {
                LanguagesLabel.Text = "Languages: -";
            }


            // Datum registracije
            if (DateTime.TryParseExact(Professor.DateOfRegistration, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime joinedDate))
            {
                JoinedDateLabel.Content = "Joined Date: " + joinedDate.ToString("dd.MM.yyyy");
            }
            else
            {
                JoinedDateLabel.Content = "Joined Date: " + Professor.DateOfRegistration;
            }
        }


        private void AddExam_Click(object sender, RoutedEventArgs e)
        {
            // TODO: implement logic for adding an appointment
        }
        private void CoursesButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void ExamsButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }
        private void ShowYourCourses_Click(object sender, RoutedEventArgs e) { }
        private void FindYourCourses_Click(object sender, RoutedEventArgs e) { }
        private void SortPaginateCourses_Click(object sender, RoutedEventArgs e) { }
        private void StartCourse_Click(object sender, RoutedEventArgs e) { }
        private void OngoingCourses_Click(object sender, RoutedEventArgs e) { }
        private void EndCourse_Click(object sender, RoutedEventArgs e) { }

        
        private void ShowYourExams_Click(object sender, RoutedEventArgs e) { }
        private void FindYourExams_Click(object sender, RoutedEventArgs e) { }
        private void SortPaginateExams_Click(object sender, RoutedEventArgs e) { }
        private void StartExam_Click(object sender, RoutedEventArgs e) { }
        private void OngoingExams_Click(object sender, RoutedEventArgs e) { }
        private void EndExam_Click(object sender, RoutedEventArgs e) { }

        private void SaveCourse_Click(object sender, RoutedEventArgs e) { }

        private void CloseAddCoursePanel_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Visibility = Visibility.Visible;
            AddCoursePanel.Visibility = Visibility.Collapsed;
        }
        private void AddCourse_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Visibility = Visibility.Collapsed;
            AddCoursePanel.Visibility = Visibility.Visible;
        }
    }
}
