using System;
using System.Windows;
using System.Windows.Controls;
using LangLang.Controller;
using LangLang.DTO;
using LangLang.Model;
using LangLang.Model.DAO;

namespace LangLang.RoleWindows
{
    /// <summary>
    /// Interaction logic for PrincipalWindow.xaml
    /// </summary>
    public partial class PrincipalWindow : Window
    {
        private ProfessorsController professorsController;
        private bool isEditMode = false;
        private ProfessorDTO currentProfessor = null;

        public PrincipalWindow(ProfessorsController professorsController)
        {
            InitializeComponent();
            DataContext = this;

            this.professorsController = professorsController;
            LoadProfessors();
        }

        private void LoadProfessors()
        {
            ProfessorsDataGrid.ItemsSource = professorsController.GetAllProfessors();
        }

        // === Professors actions ===
        private void ProfessorsButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void ShowAllProfessors_Click(object sender, RoutedEventArgs e)
        {
            LoadProfessors();
        }

        private void SearchProfessors_Click(object sender, RoutedEventArgs e)
        {
            // TODO: implement search dialog for teachers
            MessageBox.Show("Search teachers not implemented yet.");
        }

        private void SortProfessors_Click(object sender, RoutedEventArgs e)
        {
            // TODO: implement sorting logic
            MessageBox.Show("Sort teachers not implemented yet.");
        }

        private void AddProfessor_Click(object sender, RoutedEventArgs e)
        {
            isEditMode = false;
            currentProfessor = null;

            ProfessorFirstNameBox.Text = "";
            ProfessorLastNameBox.Text = "";
            ProfessorEmailBox.Text = "";
            ProfessorPhoneBox.Text = "";
            ProfessorBirthDatePicker.SelectedDate = null;
            ProfessorLanguagesBox.Text = "";

            AddEditProfessorOverlay.Visibility = Visibility.Visible;
        }

        private void UpdateProfessor_Click(object sender, RoutedEventArgs e)
        {
           
            isEditMode = false;
            currentProfessor = null;

            ProfessorFirstNameBox.Text = "";
            ProfessorLastNameBox.Text = "";
            ProfessorEmailBox.Text = "";
            ProfessorPhoneBox.Text = "";
            ProfessorBirthDatePicker.SelectedDate = null;
            ProfessorLanguagesBox.Text = "";

            AddEditProfessorOverlay.Visibility = Visibility.Visible;
        }

        private void DeleteProfessor_Click(object sender, RoutedEventArgs e)
        {
            // TODO: implement delete functionality
            MessageBox.Show("Delete teacher not implemented yet.");
        }

        private void SaveProfessor_Click(object sender, RoutedEventArgs e)
        {
            // TODO: implement save functionality
            if (isEditMode)
            {
                MessageBox.Show("Save edited teacher not implemented yet.");
            }
            else
            {
                MessageBox.Show("Save new teacher not implemented yet.");
            }

            AddEditProfessorOverlay.Visibility = Visibility.Collapsed;
            MainPanel.Visibility = Visibility.Visible;
        }

        private void CancelProfessor_Click(object sender, RoutedEventArgs e)
        {
            AddEditProfessorOverlay.Visibility = Visibility.Collapsed;
            MainPanel.Visibility = Visibility.Visible;
        }

        // === Courses ===
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

        private void SmartAssignProfessor_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Smart assign professor not implemented yet.");
        }

        // === Exams ===
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

        private void SmartAssignExaminer_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Smart assign examiner not implemented yet.");
        }

        // === Reports ===
        private void ReportsButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void ReportPenalties_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Penalty report not implemented yet.");
        }

        private void ReportCourses_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Course report not implemented yet.");
        }

        private void ReportExams_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Exam report not implemented yet.");
        }

        private void ReportLanguages_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Language report not implemented yet.");
        }
    }
}