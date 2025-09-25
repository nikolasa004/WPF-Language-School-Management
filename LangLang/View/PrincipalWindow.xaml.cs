using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LangLang.Controller.Users;
using LangLang.DTO;
using LangLang.Model;
using LangLang.View.PrincipalViewModules;

namespace LangLang.RoleWindows
{
    public partial class PrincipalWindow : Window
    {
        private ProfessorsController _professorsController;
        private PrincipalProfessorView _professorView;
        private PrincipalExamView _principalExamView;
        private PrincipalCourseView _principalCourseView;
        private PrincipalProfessorManager _professorManager;

        public PrincipalWindow(ProfessorsController _professorsController)
        {
            InitializeComponent();
         
            this._professorsController = _professorsController;

            _professorView = new PrincipalProfessorView(_professorsController, ProfessorsDataGrid, this);
            _professorManager = new PrincipalProfessorManager(_professorsController);
            _principalCourseView = new PrincipalCourseView(CoursesGrid);

            DataContext = _professorView;
            _professorView.LoadProfessors();
        }

        private void ReassignCourses_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Course reassignment functionality has been integrated into the delete professor workflow.",
                            "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CancelCourseReassignment_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Course reassignment cancelled.",
                            "Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddProfessor_Click(object sender, RoutedEventArgs e)
        {
            _professorView.ShowAddProfessorDialog(AddEditProfessorOverlay, LanguagesSection);
        }

        private void UpdateProfessor_Click(object sender, RoutedEventArgs e)
        {
            _professorView.ShowEditProfessorDialog(AddEditProfessorOverlay, LanguagesSection);
        }


        private void DeleteProfessor_Click(object sender, RoutedEventArgs e)
        {
            var selectedProfessor = _professorView.GetSelectedProfessor();
            if (selectedProfessor == null)
            {
                MessageBox.Show("Please select a professor to delete.",
                                "No selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete professor {selectedProfessor.FirstName} {selectedProfessor.LastName}?",
                "Confirm delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            bool success = _professorManager.DeleteProfessor(selectedProfessor, _professorsController);

            if (success)
            {
                MessageBox.Show("Professor successfully deleted!",
                                "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                _professorView.LoadProfessors();
            }
        }

        private void AddLanguage_Click(object sender, RoutedEventArgs e)
        {
            _professorView.AddLanguage(LanguageComboBox, LanguageLevelComboBox);
        }

        private void RemoveLanguage_Click(object sender, RoutedEventArgs e)
        {
            _professorView.RemoveLanguage(sender);
        }

        private void SaveProfessor_Click(object sender, RoutedEventArgs e)
        {
            bool success = _professorView.SaveProfessor();
            if (success)
            {
                AddEditProfessorOverlay.Visibility = Visibility.Collapsed;
            }
        }
        private void CancelProfessor_Click(object sender, RoutedEventArgs e)
        {
            AddEditProfessorOverlay.Visibility = Visibility.Collapsed;
        }

        private void CoursesButton_Click(object sender, RoutedEventArgs e)
        {
            PrincipalCourseOverlay.Visibility = Visibility.Visible;
        }
        private void SendCompliments_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Compliment messages have been sent to best students from this course.", "Send Compliments", MessageBoxButton.OK, MessageBoxImage.Information);
            Button btn = (Button)sender;
            CourseDTO course = (CourseDTO)btn.DataContext;
            string type=KnowledgeRadio.IsChecked==true?"knowledge":"activity";
            _principalCourseView.SendCompliments(course.Id, type);
            _principalCourseView.RefreshCourses();
        }
        private void ExamsButton_Click(object sender, RoutedEventArgs e)
        {
            PrincipalExamOverlay.Visibility = Visibility.Visible;

            _principalExamView = new PrincipalExamView(ExamTermsGrid);
        }
        private void ClosePrincipalExam_Click(object sender, RoutedEventArgs e)
        {
            PrincipalExamOverlay.Visibility = Visibility.Collapsed;
        }
        private void ClosePrincipalCourse_Click(object sender, RoutedEventArgs e)
        {
            PrincipalCourseOverlay.Visibility = Visibility.Collapsed;
        }
        public void ConfirmResults_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ExamTermDTO exam)
            {
                MessageBox.Show(
                    $"Results for {exam.Language} {exam.Level} exam on {exam.Date} at {exam.StartAt} confirmed.",
                    "Confirm Results",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                _principalExamView.ConfirmResult(exam.Id);
                _principalExamView.RefreshExams();
            }
        }

        private void ReportsButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO
        }
        private void ProfessorsButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            _professorView.SearchProfessors(ExamLanguageCombo, ExamLevelCombo, ExamFromDate, ExamToDate);
        }

        private void RemoveFilters_Click(object sender, RoutedEventArgs e)
        {
            _professorView.RemoveFilters(ExamLanguageCombo, ExamLevelCombo, ExamFromDate, ExamToDate);
        }
    }
}