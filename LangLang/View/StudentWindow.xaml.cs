using LangLang.Controller.Users;
using LangLang.DTO;
using LangLang.Model.Users;
using LangLang.RoleWindows.StudentViewModules;
using LangLang.View;
using System.Windows;
using System.Windows.Controls;

namespace LangLang.RoleWindows
{
    public partial class StudentWindow : Window
    {
        public StudentDTO Student { get; set; }
        public StudentDTO UpdateStudent { get; set; }
        private StudentsController studentsController;

        private StudentViewExams studentViewExams;
        private StudentViewCourses studentViewCourses;
        private StudentViewProfile studentViewProfile;

        private string currentCoursePanel= string.Empty;

        public StudentWindow(Student student,StudentsController studentsController)
        {
            InitializeComponent();
            DataContext = this;
            Student = new StudentDTO(student);
            UpdateStudent = new StudentDTO(student);
            this.studentsController = studentsController;

            studentViewExams = new StudentViewExams(ExamLanguageCombo, ExamLevelCombo, ExamFromDate, ExamToDate, ExamsGrid);
            studentViewCourses = new StudentViewCourses(CourseLanguageCombo, CourseLevelCombo, CourseOnlineCombo, CourseWeeksBox, CourseFromDate, CourseToDate, CoursesGrid, Student.Email, studentsController);
            studentViewProfile = new StudentViewProfile(Student, studentsController, FirstNameBox, LastNameBox, MaleRadioButton, FemaleRadioButton, BirthDatePicker, PhoneBox, EmailBox, PasswordTextBox, EducationComboBox);
            studentViewProfile.SetLabels(NameLabel, SurnameLabel, GenderLabel, BirthDateLabel, PhoneLabel, EmailLabel, EducationLabel, CurrentCourseLabel, PenaltyPointsLabel,JoinedDateLabel);
            InformationUpdate();
        }
        private void ActivityButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.IsOpen = true;
            }
        }

        // Profile Panel
        private void InformationUpdate()
        {
            studentViewProfile.InformationUpdate();
        }
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            studentViewProfile.SetStudentData();

            MainPanel.Visibility = Visibility.Collapsed;
            EditPanel.Visibility = Visibility.Visible;
        }
        private void SaveEditChanges_Click(object sender, RoutedEventArgs e)
        {
            if(studentViewProfile.SaveEditChanges(UpdateStudent))
            {
                Student student = UpdateStudent.ToStudent();
                Student = new StudentDTO(student);
                CloseEditPanel_Click(sender, e);
            }
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if(studentViewProfile.DeleteProfile())
            {
                Close();
            }
        }
        private void CloseEditPanel_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Visibility = Visibility.Visible;
            EditPanel.Visibility = Visibility.Collapsed;
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
        private void OpenCoursesPanel_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Visibility = Visibility.Collapsed;
            CoursesPanel.Visibility = Visibility.Visible;
        }
        private void CloseCoursesPanel_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Visibility = Visibility.Visible;
            CoursesPanel.Visibility = Visibility.Collapsed;
            currentCoursePanel = string.Empty;
        }
        private void ShowAllCourses_Click(object sender, RoutedEventArgs e)
        {
            currentCoursePanel = "show all";
            CoursePanelButton.Content = "Enroll";
            studentViewCourses.ShowAllCourses();
            OpenCoursesPanel_Click(sender, e);
        }
        private void ShowRequestedCourses_Click(object sender, RoutedEventArgs e)
        {
            currentCoursePanel = "show requested";
            CoursePanelButton.Content = "Unenroll";
            studentViewCourses.ShowRequestedCourses();
            OpenCoursesPanel_Click(sender, e);
        }
        private void CoursesSearch_Click(object sender, RoutedEventArgs e)
        {
            studentViewCourses.SearchCourses();
        }
        private void CoursePanelButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentCoursePanel == "show all")
            {
                EnrollCourse_Click();
            }
            else if (currentCoursePanel == "show requested")
            {
                UnenrollCourse_Click();
            }
        }
        private void EnrollCourse_Click()
        {
            studentViewCourses.EnrollInSelectedCourse();
        }
        private void UnenrollCourse_Click()
        {
            studentViewCourses.UnenrollFromSelectedCourse();
        }
        private void PrevPageCourses_Click(object sender, RoutedEventArgs e)
        {
            studentViewCourses.PrevPage();
        }
        private void NextPageCourses_Click(object sender, RoutedEventArgs e)
        {
            studentViewCourses.NextPage();
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

        //Inbox Panel
        private void OpenInboxPanel_Click(object sender, RoutedEventArgs e)
        {
            MessagesInboxWindow messagesInboxWindow = new MessagesInboxWindow(Student.Email);
            messagesInboxWindow.Show();
        }
    }
}