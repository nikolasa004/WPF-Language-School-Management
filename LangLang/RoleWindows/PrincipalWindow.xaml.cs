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

        public PrincipalWindow(ProfessorsController professorsController)
        {
            InitializeComponent();
            DataContext = this;

            this.professorsController = professorsController;
            LoadTeachers();
        }

        private void LoadTeachers()
        {
            TeachersDataGrid.ItemsSource = professorsController.GetAllProfessors();
        }

        // === Teachers actions ===
        private void ShowAllTeachers_Click(object sender, RoutedEventArgs e)
        {
            LoadTeachers();
        }

        private void SearchTeachers_Click(object sender, RoutedEventArgs e)
        {
            // TODO: implement search dialog for teachers
            MessageBox.Show("Search teachers not implemented yet.");
        }

        private void SortTeachers_Click(object sender, RoutedEventArgs e)
        {
            // TODO: implement sorting logic
            MessageBox.Show("Sort teachers not implemented yet.");
        }

        private void AddTeacher_Click(object sender, RoutedEventArgs e)
        {
            // TODO: open AddTeacher dialog
            MessageBox.Show("Add teacher not implemented yet.");
        }

        private void UpdateTeacher_Click(object sender, RoutedEventArgs e)
        {
            // TODO: open UpdateTeacher dialog
            MessageBox.Show("Update teacher not implemented yet.");
        }

        private void DeleteTeacher_Click(object sender, RoutedEventArgs e)
        {
            // TODO: open UpdateTeacher dialog
            MessageBox.Show("Delete teacher not implemented yet.");
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
        private void TeachersButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }
    }
}
