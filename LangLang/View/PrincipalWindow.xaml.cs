using LangLang.Controller.Messages;
using LangLang.Controller.TeachingActivities;
using LangLang.Controller.Users;
using LangLang.DTO;
using LangLang.Utils;
using LangLang.View.PrincipalViewModules;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LangLang.RoleWindows
{
    public partial class PrincipalWindow : Window
    {
        private ProfessorsController _professorsController;
        private PrincipalProfessorView _professorView;
        private PrincipalExamView _principalExamView;
        private PrincipalCourseView _principalCourseView;
        private PrincipalProfessorManager _professorManager;
        private readonly CourseController _courseController = new CourseController();
        private PrincipalBrowseCourses _principalBrowseCourses;
        private StudentsController studentsController;
        private ExamResultsController examResultsController;

        private ReportsController _reportsService;

        public PrincipalWindow(ProfessorsController _professorsController)
        {
            InitializeComponent();
            this._professorsController = _professorsController;
            _professorView = new PrincipalProfessorView(_professorsController, ProfessorsDataGrid, this);
            _professorManager = new PrincipalProfessorManager(_professorsController);
            _principalCourseView = new PrincipalCourseView(CoursesGrid);
            studentsController = new StudentsController();
            examResultsController = new ExamResultsController();
            _reportsService = new ReportsController(studentsController, examResultsController,_courseController, _professorsController);
            _principalBrowseCourses = new PrincipalBrowseCourses(
                PrincipalCourseLanguageCombo, PrincipalCourseLevelCombo, PrincipalCourseOnlineCombo,
                PrincipalCourseWeeksBox, PrincipalCourseFromDate, PrincipalCourseToDate,
                PrincipalBrowseCoursesGrid, PrincipalPagerTextCourses
            );
            DataContext = _professorView;
            _professorView.LoadProfessors();
        }
        private void DropDown_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            btn.ContextMenu.PlacementTarget = btn;
            btn.ContextMenu.IsOpen = true;
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
            string type = KnowledgeRadio.IsChecked == true ? "knowledge" : "activity";
            _principalCourseView.SendCompliments(course.Id, type);
            _principalCourseView.RefreshCourses();
        }
        private void ClosePrincipalCourse_Click(object sender, RoutedEventArgs e)
        {
            PrincipalCourseOverlay.Visibility = Visibility.Collapsed;
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
        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            MenuItem btn = (MenuItem)sender;
            string reportType = (string)btn.Tag;
            _reportsService.GenerateReport(reportType);
            MessageBox.Show("Report generated and sent to your email.", "Report Generated", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void ProfessorsButton_Click(object sender, RoutedEventArgs e) { /* TODO */ }
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            _professorView.SearchProfessors(ExamLanguageCombo, ExamLevelCombo, ExamFromDate, ExamToDate);
        }
        private void RemoveFilters_Click(object sender, RoutedEventArgs e)
        {
            _professorView.RemoveFilters(ExamLanguageCombo, ExamLevelCombo, ExamFromDate, ExamToDate);
        }
        private void AvailableCourse_Click(object sender, RoutedEventArgs e) { }
        private void TeachingActivity_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Visibility = Visibility.Collapsed;
            PrincipalCoursesPanel.Visibility = Visibility.Visible;
            _principalBrowseCourses.Refresh(resetPage: true);
        }
        private void CloseCoursesPanel_Click(object sender, RoutedEventArgs e)
        {
            PrincipalCoursesPanel.Visibility = Visibility.Collapsed;
            MainPanel.Visibility = Visibility.Visible;
        }
        private void CoursesSearch_Click(object sender, RoutedEventArgs e)
        {
            _principalBrowseCourses.SearchCourses(sender, e);
        }
        private void CoursesGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            _principalBrowseCourses.CoursesGrid_Sorting(sender, e);
        }
        private void PrevPageCourses_Click(object sender, RoutedEventArgs e)
        {
            _principalBrowseCourses.PrevPage();
        }
        private void NextPageCourses_Click(object sender, RoutedEventArgs e)
        {
            _principalBrowseCourses.NextPage();
        }
        private void PageSizeComboCourses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _principalBrowseCourses.ChangePageSize(sender, e);
        }
        private void AddCourseButton_Click(object sender, RoutedEventArgs e)
        {
            PrincipalAddCourseOverlay.Visibility = Visibility.Visible;
        }
        private void CancelAddCourse_Click(object sender, RoutedEventArgs e)
        {
            PrincipalAddCourseOverlay.Visibility = Visibility.Collapsed;
        }
        private void SaveNewCourse_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Adding courses is not available for Principal in this version.",
                            "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            PrincipalAddCourseOverlay.Visibility = Visibility.Collapsed;
        }
        private void EditCourse_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is CourseDTO)
            {
                PrincipalEditCourseOverlay.Visibility = Visibility.Visible;
            }
        }
        private void CancelEditCourse_Click(object sender, RoutedEventArgs e)
        {
            PrincipalEditCourseOverlay.Visibility = Visibility.Collapsed;
        }
        private void SaveEditedCourse_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Editing courses is not available for Principal in this version.",
                            "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            PrincipalEditCourseOverlay.Visibility = Visibility.Collapsed;
        }
        private void DeleteCourse_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Deleting courses is not available for Principal in this version.",
                            "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void StartCourseButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Starting a course is available to Professors only.",
                            "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void OngoingCourseButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Opening an ongoing course is available to Professors only.",
                            "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void AssignProfessor_Click(object sender, RoutedEventArgs e)
        {
            if (PrincipalBrowseCoursesGrid.SelectedItem is not CourseDTO dto)
            {
                MessageBox.Show("Please select a course first.", "Assign professor",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (!string.IsNullOrWhiteSpace(dto.ProfessorId))
            {
                MessageBox.Show("This course already has a professor.", "Assign professor",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var allProfs = _professorsController.GetAllProfessors();   
            var allCourses = _courseController.GetAll();
            var course = dto.ToCourse();
            var days = course.Days.Select(d => d == DayOfWeek.Sunday ? 7 : (int)d);
            var chosen = AutoAssignHelpers.SmartPickProfessor(
                allProfs, allCourses, course.Language, course.Level, days, course.StartAt!);
            if (chosen == null)
            {
                MessageBox.Show("No available qualified professor for this schedule.",
                                "Assign professor", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            course.ProfessorId = chosen.Email;
            _courseController.Update(course);
            MessageBox.Show($"Assigned: {chosen.FirstName} {chosen.LastName}.",
                            "Assign professor", MessageBoxButton.OK, MessageBoxImage.Information);
            _principalBrowseCourses.Refresh();
        }
    }
}
