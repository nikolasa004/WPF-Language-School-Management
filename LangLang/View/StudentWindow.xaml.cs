using LangLang.Controller.Messages;
using LangLang.Controller.Requests;
using LangLang.Controller.TeachingActivities;
using LangLang.Controller.Users;
using LangLang.DTO;
using LangLang.Model.Users;
using LangLang.RoleWindows.StudentViewModules;
using LangLang.Utils;
using LangLang.View;
using LangLang.View.StudentViewModules;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace LangLang.RoleWindows
{
    public partial class StudentWindow : Window
    {
        public StudentDTO Student { get; set; }
        public StudentDTO UpdateStudent { get; set; }

        private StudentsController studentsController;
        private CourseController courseController=new CourseController();
        private CourseEnrollmentsController enrollmentsController=new CourseEnrollmentsController();
        private ExamTermsController examController=new ExamTermsController();
        private ExamEnrollmentsController examEnrollmentsController=new ExamEnrollmentsController();
        private MessagesController messagesController=new MessagesController();

        private StudentViewExams studentViewExams;
        private StudentViewCourses studentViewCourses;
        private StudentViewProfile studentViewProfile;
        private StudentViewEdit studentViewEdit;
        private StudentViewTeaching studentViewTeaching;

        private List<Grid> panels;

        public StudentWindow(Student student, StudentsController studentsController)
        {
            InitializeComponent();
            DataContext = this;
            this.studentsController = studentsController;

            studentViewExams = new StudentViewExams(examController,examEnrollmentsController,this.studentsController,ExamLanguageCombo,ExamLevelCombo,ExamFromDate,ExamToDate,ExamsGrid,student.Email);
            studentViewCourses = new StudentViewCourses(courseController,enrollmentsController,CourseLanguageCombo,CourseLevelCombo,CourseOnlineCombo,CourseWeeksBox,CourseFromDate,CourseToDate,CoursesGrid,student.Email,this.studentsController,messagesController);
            RateWindow? rateWindow = studentViewCourses.CheckIfActiveCourseFinished();

            Student = new StudentDTO(student);
            UpdateStudent = new StudentDTO(student);
            panels = new List<Grid> { MainPanel, EditPanel, ExamsPanel, CoursesPanel };
            Dictionary<string, Action> coursePanelActions = new Dictionary<string, Action>{{ "show all",() => { CoursePanelButton.Content = "Enroll";studentViewCourses.ShowAllCourses();}},{ "show requested",() => { CoursePanelButton.Content = "Cancel";studentViewCourses.ShowRequestedCourses(); } },{ "show active",() => { CoursePanelButton.Content = "Unenroll";studentViewCourses.ShowActiveCourse();} }};
            Dictionary<string, Action> examPanelActions = new Dictionary<string, Action> { { "show all", () => { ExamPanelButton.Content = "Enroll"; studentViewExams.ShowAllExams(); } }, { "show requested", () => { ExamPanelButton.Content = "Cancel"; studentViewExams.ShowRequestedExams(); } }, { "show active", () => { ExamPanelButton.Content = "Cancel"; studentViewExams.ShowActiveExam(); } } };

            studentViewProfile = new StudentViewProfile(Student,this.studentsController, courseController,enrollmentsController,examEnrollmentsController,messagesController);
            studentViewProfile.SetLabels(NameLabel,SurnameLabel,GenderLabel,BirthDateLabel,PhoneLabel,EmailLabel,EducationLabel,CurrentCourseLabel,PenaltyPointsLabel,JoinedDateLabel);
            studentViewEdit = new StudentViewEdit(Student,this.studentsController, studentViewProfile, FirstNameBox, LastNameBox, MaleRadioButton, FemaleRadioButton, BirthDatePicker, PhoneBox, EmailBox, PasswordTextBox, EducationComboBox);
            studentViewTeaching = new StudentViewTeaching(panels, coursePanelActions, examPanelActions, studentViewCourses, studentViewExams);
            studentViewProfile.InformationUpdate();
            if (rateWindow != null)
            {
                rateWindow.Show();
                rateWindow.Topmost = true;
            }
            SetTeachingButtons();
        }
        private void ActivityButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.IsOpen = true;
            }
        }
        private void ChangePanel_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string panelIndex)
                GUIHelper.ChangePanel(panels, int.Parse(panelIndex));
        }
        private void TeachingActivity_Click(object sender, RoutedEventArgs e)
        {
            var guiItem = (Control)sender!;
            string methodName = guiItem.Tag!.ToString()!;
            var method = studentViewTeaching.GetType().GetMethod(methodName)!;
            method.Invoke(studentViewTeaching, null);
        }

        // Profile Panel
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (Student.ActiveCourseId != -1)
            {
                MessageBox.Show("You cannot edit your profile while enrolled in a course. Please finish or leave your current course before making changes.","Edit Profile",MessageBoxButton.OK,MessageBoxImage.Warning);
                return;
            }
            if (Student.ActiveExamTermId != -1)
            {
                MessageBox.Show("You cannot edit your profile after you have applied for an exam.","Edit Profile", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            studentViewEdit.SetStudentData();
            GUIHelper.ChangePanel(panels, 1);
        }
        private void SaveEditChanges_Click(object sender, RoutedEventArgs e)
        {
            if (studentViewEdit.SaveEditChanges(UpdateStudent))
            {
                Student student = UpdateStudent.ToStudent();
                Student = new StudentDTO(student);
                GUIHelper.ChangePanel(panels, 0);
            }
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (studentViewProfile.DeleteProfile())
                Close();
        }
        private void OpenInboxPanel_Click(object sender, RoutedEventArgs e)
        {
            MessagesInboxWindow messagesInboxWindow = new MessagesInboxWindow(Student.Email, messagesController);
            messagesInboxWindow.Show();
        }

        //Teaching Activities
        private void SetTeachingButtons()
        {
            PrevPageBtnExams.Click += (s, e) => studentViewExams.PrevPage();
            NextPageBtnExams.Click += (s, e) => studentViewExams.NextPage();
            ExamsSearch.Click += (s, e) => studentViewExams.SearchExams();
            FinishedExams.Click += (s, e) => studentViewExams.ShowFinishedExams();
            PrevPageBtnCourses.Click += (s, e) => studentViewCourses.PrevPage();
            NextPageBtnCourses.Click += (s, e) => studentViewCourses.NextPage();
            CoursesSearch.Click += (s, e) => studentViewCourses.SearchCourses();
            FinishedCourses.Click += (s, e) => studentViewCourses.ShowFinishedCourses();
        }
        private void PageSizeComboExams_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (studentViewExams == null) return;
            studentViewExams.PageSizeChanged(sender, e);
        }
        private void ExamsGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            studentViewExams.ExamsGridSorting(sender, e);
        }
        private void PageSizeComboCourses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (studentViewCourses == null) return;
            studentViewCourses.PageSizeChanged(sender);
        }
        private void CoursesGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            studentViewCourses.CoursesGridSorting(e);
        }
    }
}