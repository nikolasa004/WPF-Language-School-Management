using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using LangLang.Controller;
using LangLang.DTO;
using LangLang.Model;
using LangLang.RoleWindows.ProfessorExamModules;     
using LangLang.RoleWindows.ProfessorViewModules;

namespace LangLang.RoleWindows
{
    public partial class ProfessorWindow : Window
    {
        private ProfessorViewProfile _profileView;

        private ProfessorViewCourses professorViewCourses;
        private ProfessorManageCourses _manageCourses;
        private List<CourseDTO> _allCourses = new();

        private ProfessorViewExams _examsView;
        private ProfessorManageExams _examsManage;

        public ProfessorDTO Professor { get; set; }
        public ProfessorDTO UpdateProfessor { get; set; }

        private readonly ProfessorsController professorsController;
        private readonly ExamTermsController _examController = new();
        private readonly CourseController _courseController = new();
        private readonly string _professorId;

        public ProfessorWindow(Professor professor, ProfessorsController professorsController)
        {
            InitializeComponent();

            var sr = new CultureInfo("sr-RS");
            Thread.CurrentThread.CurrentCulture = sr;
            Thread.CurrentThread.CurrentUICulture = sr;
            this.Language = XmlLanguage.GetLanguage(sr.IetfLanguageTag);

            DataContext = this;

            Professor = new ProfessorDTO(professor);
            UpdateProfessor = new ProfessorDTO(professor);

            _profileView = new ProfessorViewProfile(Professor);
            _profileView.SetLabels(NameLabel, SurnameLabel, GenderLabel, BirthDateLabel, PhoneLabel,
                                   EmailLabel, EducationLabel, LanguagesLabel, JoinedDateLabel);
            _profileView.InformationUpdate();

            this.professorsController = professorsController;
            _professorId = Professor.Email;
            _examsView = new ProfessorViewExams(
                _professorId,
                _examController,
                ExamsGrid,
                ExamLanguageCombo, ExamLevelCombo,
                ExamFromDate, ExamToDate,
                (TextBlock)FindName("PagerText"),
                PageSizeCombo);

            _examsView.InitFilters(sr, this.Language);
            _examsView.ReloadAll();

            _examsManage = new ProfessorManageExams(
                _professorId,
                _examController,
                _examsView,
                AddExamOverlay, AddExamLangCombo, AddExamLevelCombo, AddExamDatePicker, AddExamTimeBox, AddExamMaxBox, AddExamRoomBox,
                EditExamOverlay, EditExamLangCombo, EditExamLevelCombo, EditExamDatePicker, EditExamTimeBox, EditExamMaxBox, EditExamRoomBox);

            _allCourses = _courseController.GetMyCourses(_professorId)
                  .Select(c => new CourseDTO(c)).ToList();

            professorViewCourses = new ProfessorViewCourses(
                _allCourses,
                CourseLanguageCombo, CourseLevelCombo, CourseOnlineCombo, CourseWeeksBox,
                CourseFromDate, CourseToDate, CoursesGrid);
            professorViewCourses.ApplyFiltersSortAndPageForCourses();

            _manageCourses = new ProfessorManageCourses(
                _professorId,
                _allCourses,
                professorViewCourses,
                AddCourseOverlay,
                AddCourseLangCombo, AddCourseLevelCombo,
                AddCourseMaxBox, AddCourseRoomBox,
                AddCourseDatePicker, AddCourseTimeBox,
                AddCourseIsLiveCheckBox,
                AddCourseMonCheckBox, AddCourseTueCheckBox, AddCourseWedCheckBox,
                AddCourseThuCheckBox, AddCourseFriCheckBox, AddCourseSatCheckBox, AddCourseSunCheckBox,
                AddCourseDurationWeeksBox,
                EditCourseOverlay,
                EditCourseLangCombo, EditCourseLevelCombo,
                EditCourseMaxBox, EditCourseRoomBox,
                EditCourseDatePicker, EditCourseTimeBox,
                EditCourseIsLiveCheckBox,
                EditCourseMonCheckBox, EditCourseTueCheckBox, EditCourseWedCheckBox,
                EditCourseThuCheckBox, EditCourseFriCheckBox, EditCourseSatCheckBox, EditCourseSunCheckBox,
                EditCourseDurationWeeksBox);
        }
        private void ExamsButton_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Visibility = Visibility.Collapsed;
            AddCourseOverlay.Visibility = Visibility.Collapsed;
            ExamsPanel.Visibility = Visibility.Visible;

            _examsView.InitFilters(new CultureInfo("sr-RS"), this.Language);
            _examsView.ReloadAll();
        }

        private void CloseExamsPanel_Click(object sender, RoutedEventArgs e)
        {
            ExamsPanel.Visibility = Visibility.Collapsed;
            AddCourseOverlay.Visibility = Visibility.Collapsed;
            MainPanel.Visibility = Visibility.Visible;
        }

        private void FindYourExams_Click(object sender, RoutedEventArgs e) => _examsView.ApplyFiltersSortAndPage(true);
        private void ExamsGrid_Sorting(object sender, DataGridSortingEventArgs e) => _examsView.GridSorting(sender, e);
        private void PrevPage_Click(object? sender, RoutedEventArgs e) => _examsView.PrevPage(sender, e);
        private void NextPage_Click(object? sender, RoutedEventArgs e) => _examsView.NextPage(sender, e);
        private void PageSizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e) => _examsView.PageSizeChanged(sender, e, IsLoaded);

        private void AddExam_Click(object sender, RoutedEventArgs e) => _examsManage.OpenAddOverlay(this);
        private void SaveNewExam_Click(object sender, RoutedEventArgs e) => _examsManage.SaveNewExam(this);
        private void CancelNewExam_Click(object sender, RoutedEventArgs e) => _examsManage.CloseAddExamOverlay();

        private void EditExam_Click(object sender, RoutedEventArgs e) => _examsManage.EditExam_Click(this, sender, e);
        private void SaveEditedExam_Click(object sender, RoutedEventArgs e) => _examsManage.SaveEditedExam();
        private void CancelEditExam_Click(object sender, RoutedEventArgs e) => _examsManage.CloseEditExamOverlay();

        private void CancelExam_Click(object sender, RoutedEventArgs e) => _examsManage.CancelExam(sender, e);

        private void SaveNewCourse_Click(object sender, RoutedEventArgs e) => _manageCourses.SaveNewCourse();
        private void CancelAddCourse_Click(object sender, RoutedEventArgs e) => _manageCourses.CloseAddCourseOverlay();
        private void DeleteCourse_Click(object sender, RoutedEventArgs e) { if ((sender as FrameworkElement)?.DataContext is CourseDTO course) _manageCourses.DeleteCourse(course); }
        private void SaveEditedCourse_Click(object sender, RoutedEventArgs e) => _manageCourses.SaveEditedCourse();
        private void AddCourseButton_Click(object sender, RoutedEventArgs e) => _manageCourses.OpenAddCourseOverlay();
        private void EditCourse_Click(object sender, RoutedEventArgs e) { if ((sender as FrameworkElement)?.DataContext is CourseDTO course) _manageCourses.OpenEditCourseOverlay(course); }
        private void CancelEditCourse_Click(object sender, RoutedEventArgs e) => _manageCourses.CloseEditCourseOverlay();
        private void CoursesSearch_Click(object sender, RoutedEventArgs e) => professorViewCourses.SearchCourses(sender, e);
        private void PrevPageCourses_Click(object sender, RoutedEventArgs e) => professorViewCourses.PrevPageForCourses(sender, e);
        private void NextPageCourses_Click(object sender, RoutedEventArgs e) => professorViewCourses.NextPageForCourses(sender, e);
        private void PageSizeComboCourses_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (professorViewCourses != null) professorViewCourses.PageSizeChanged(sender, e); }
        private void CoursesGrid_Sorting(object sender, DataGridSortingEventArgs e) => professorViewCourses.CoursesGridSorting(sender, e);
        private void CoursesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                professorViewCourses?.InitCourseFilters();
                CoursesPanel.Visibility = Visibility.Visible;
                MainPanel.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška pri otvaranju Courses panela:\n" + ex);
            }
        }
        private void CloseCoursesPanel_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Visibility = Visibility.Visible;
            CoursesPanel.Visibility = Visibility.Collapsed;
            professorViewCourses?.ApplyFiltersSortAndPageForCourses(resetPage: true);
        }
    }
}
