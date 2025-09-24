using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using LangLang.Controller.Messages;
using LangLang.Controller.Services;
using LangLang.Controller.TeachingActivities;
using LangLang.Controller.Users;
using LangLang.DTO;
using LangLang.Model.Enums;
using LangLang.Model.TeachingActivities;
using LangLang.Model.Users;
using LangLang.RoleWindows.ProfessorExamModules;
using LangLang.RoleWindows.ProfessorViewModules;
using LangLang.View.ProfessorViewModules;

namespace LangLang.RoleWindows
{
    public partial class ProfessorWindow : Window
    {
        private readonly MessagesController _messagesController;

        private ProfessorFinishCourse _finishCourseView;

        private ProfessorUnenrollRequests _unenrollRequestsView;

        private readonly StudentsController _studentController;
        private List<StudentDTO> _allStudents;
        private readonly ObservableCollection<StudentDTO> _studentsObservable = new();
        private int _currentCourseId;
     

        private ProfessorViewProfile _profileView;

        private ProfessorViewCourses professorViewCourses;
        private ProfessorManageCourses _manageCourses;
        private List<CourseDTO> _allCourses = new();

        private ProfessorViewExams _examsView;
        private ProfessorManageExams _examsManage;

        private ProfessorStartExam _examStartView;

        public ProfessorDTO Professor { get; set; }
        public ProfessorDTO UpdateProfessor { get; set; }

        private readonly ProfessorsController professorsController;
        private readonly ExamTermsController _examController = new();
        private readonly CourseController _courseController = new();
        private readonly string _professorId;

        private ProfessorOngoingCourses _ongoingView;

        private ProfessorActionsExam _examActions;
        private readonly ExamButtonService _examButtonService = new ExamButtonService();


        public ProfessorWindow(Professor professor, ProfessorsController professorsController)
        {
            InitializeComponent();

            _messagesController = new MessagesController(); 

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

            StudentsController studentsController = new StudentsController();
            _allStudents = studentsController.GetAllStudents()
                             .Select(s => new StudentDTO(s))
                             .ToList();

            _studentController = new StudentsController();
            _allStudents = _studentController.GetAllStudents()
                                 .Select(s => new StudentDTO(s))
                                 .ToList();
            _unenrollRequestsView = new ProfessorUnenrollRequests(
                UnenrollRequestsGrid,   
                _studentController,
                _courseController,
                _messagesController
            );

            _examActions = new ProfessorActionsExam();
        }
        private ProfessorStartCourses _studentsView;
        private ProfessorStartExam _startExamView;

        private void RejectStudent_Click(object sender, RoutedEventArgs e)
        {
            if (_studentsView != null)
                _studentsView.RejectStudent_Click(sender, e);
        }
        private void ExamsButton_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Visibility = Visibility.Collapsed;
            AddCourseOverlay.Visibility = Visibility.Collapsed;
            ExamsPanel.Visibility = Visibility.Visible;

            _examsView.InitFilters(new CultureInfo("sr-RS"), this.Language);
            _examsView.ReloadAll();
        }
        private void CustomExamButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ExamTermDTO exam)
            {
                switch (button.Tag)
                {
                    case ExamButtonState.Start:
                        _examStartView = new ProfessorStartExam(
                        _allStudents.Where(s => s.RequestedExamTerms.Contains(exam.Id) && !s.IsExamApplicationLocked).ToList(),
                        ExamStudentsGrid,
                            exam.Id,
                            _studentController,
                            _examController
                        );
                        StartExamOverlay.Visibility = Visibility.Visible;
                        break;

                    case ExamButtonState.Open:
                        _examActions.OpenExam();
                        break;

                    case ExamButtonState.Finish:
                        _examActions.FinishExam();
                        break;
                }
            }
        }

        private void CloseStartExam_Click(object sender, RoutedEventArgs e)
        {
            StartExamOverlay.Visibility = Visibility.Collapsed;
        }

        private void StartExam_Click(object sender, RoutedEventArgs e)
        {
            if (_examStartView?.ConfirmExamStart() == true)
            {
                StartExamOverlay.Visibility = Visibility.Collapsed;

            }
        }

        private void OngoingCourseButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is CourseDTO course)
            {
                _currentCourseId = course.Id;
                OngoingCoursePanel.Visibility = Visibility.Visible;

                _ongoingView = new ProfessorOngoingCourses(
                    _allStudents,
                    StudentsGridProfessor,          
                    _currentCourseId,
                    course,
                    _messagesController
                );
            }
        }

        private void CloseOngoingCourse_Click(object sender, RoutedEventArgs e) {
            OngoingCoursePanel.Visibility = Visibility.Collapsed;
        }

        private void FinishCourse_Click(object sender, RoutedEventArgs e)
        {
            FinishCoursePanel.Visibility = Visibility.Visible;

       
            _finishCourseView = new ProfessorFinishCourse(_allStudents, FinishCourseGrid, _currentCourseId);
        }

        private void BackToOngoingCourse_Click(object sender, RoutedEventArgs e)
        {
            FinishCoursePanel.Visibility = Visibility.Collapsed;
        }

        private void ConfirmFinishCourse_Click(object sender, RoutedEventArgs e)
        {
            if (_finishCourseView != null)
            {
                bool success = _finishCourseView.FinishCourseAndGradeStudents(_currentCourseId, _courseController, FinishCourseGrid, _studentController);

                if (success)
                {
                    MessageBox.Show("Course successfully finished and grades recorded.");
                    FinishCoursePanel.Visibility = Visibility.Collapsed;

                  
                    _finishCourseView.RefreshStudents(new List<StudentDTO>());
                }
            }
        }
        private void ExpelStudent_Click(object sender, RoutedEventArgs e)
        {
            if (_ongoingView != null)
            {
                _ongoingView.ExpelStudent_Click(sender, e);
            }
        }

        private void GivePenaltyPoints_Click(object sender, RoutedEventArgs e)
        {
            if (_ongoingView != null)
            {
                _ongoingView.AddPenalty_Click(sender, e);
            }
        }


        private void CloseExamsPanel_Click(object sender, RoutedEventArgs e)
        {
            ExamsPanel.Visibility = Visibility.Collapsed;
            AddCourseOverlay.Visibility = Visibility.Collapsed;
            MainPanel.Visibility = Visibility.Visible;
        }
        private void ViewUnenrollmentRequests_Click(object sender, RoutedEventArgs e) {
            UnenrollRequestsPanel.Visibility = Visibility.Visible;
        }
        private void ApproveUnenroll_Click(object sender, RoutedEventArgs e)
        {
            _unenrollRequestsView?.ApproveUnenroll_Click(sender, e);
        }

        private void RejectUnenroll_Click(object sender, RoutedEventArgs e)
        {
            _unenrollRequestsView?.RejectUnenroll_Click(sender, e);
        }

        private void CloseUnenrollRequests_Click(object sender, RoutedEventArgs e) {
            UnenrollRequestsPanel.Visibility = Visibility.Collapsed;
        }


        private void FindYourExams_Click(object sender, RoutedEventArgs e) => _examsView.ApplyFiltersSortAndPage(true);
        private void ExamsGrid_Sorting(object sender, DataGridSortingEventArgs e) => _examsView.GridSorting(sender, e);
        private void PrevPage_Click(object? sender, RoutedEventArgs e) => _examsView.PrevPage(sender, e);
        private void NextPage_Click(object? sender, RoutedEventArgs e) => _examsView.NextPage(sender, e);
        private void PageSizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_examsView == null)
                return;
            _examsView.PageSizeChanged(sender, e, IsLoaded);
            
        }
        private void StartCourseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is CourseDTO course)
            {
              
                if (DateTime.TryParseExact(course.Date, "dd.MM.yyyy", CultureInfo.InvariantCulture,
                                           DateTimeStyles.None, out DateTime courseDate))
                {
                    var daysUntilStart = (courseDate - DateTime.Now.Date).TotalDays;
                    if (daysUntilStart > 7)
                    {
                        MessageBox.Show("Cannot view students more than 7 days before the course starts.",
                                        "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Invalid course date.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

         
                bool courseAlreadyStarted = _allStudents.Any(s => s.ActiveCourseId == course.Id);
                if (courseAlreadyStarted)
                {
                    MessageBox.Show("This course has already been started. You cannot modify student requests.",
                                    "Course Already Started", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _currentCourseId = course.Id;

                StartCourseOverlay.Visibility = Visibility.Visible;

                _studentsView = new ProfessorStartCourses(
                    _allStudents.Where(s => s.RequestedCourses.Contains(_currentCourseId)).ToList(),
                    StudentsGrid,
                    _currentCourseId,
                    course,
                    _messagesController
                );
            }
        }



        private void CloseStartCourse_Click(object sender, RoutedEventArgs e)
        {
            StartCourseOverlay.Visibility = Visibility.Collapsed;

        }

        private void StartCourse_Click(object sender, RoutedEventArgs e)
        {
            if (_studentsView != null)
            {
                _studentsView.StartCourse_Click(sender, e);
                StartCourseOverlay.Visibility = Visibility.Collapsed;
            }
        }


        private void StartStudent_Click(object sender, SelectionChangedEventArgs e) 
        { 
            if(_examsView == null)
                return;
            _examsView.PageSizeChanged(sender, e, IsLoaded);
        }

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
        private void EditCourse_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is CourseDTO course)
            {

                bool courseAlreadyStarted = _allStudents.Any(s => s.ActiveCourseId == course.Id);
                if (courseAlreadyStarted)
                {
                    MessageBox.Show("This course has already been started. Editing is not allowed.",
                                    "Course Already Started", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _manageCourses.OpenEditCourseOverlay(course);
            }
        }
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
