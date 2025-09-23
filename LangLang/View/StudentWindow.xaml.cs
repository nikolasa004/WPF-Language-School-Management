using LangLang.Controller.TeachingActivities;
using LangLang.Controller.Users;
using LangLang.DTO;
using LangLang.Model.Users;
using LangLang.RoleWindows.StudentViewModules;
using LangLang.View;
using System.Windows;
using System.Windows.Controls;
using LangLang.Controller.Messages;
using LangLang.Controller.Requests;
using System;

namespace LangLang.RoleWindows
{
    public partial class StudentWindow : Window
    {
        public StudentDTO Student { get; set; }
        public StudentDTO UpdateStudent { get; set; }

        private StudentsController studentsController;

        // --- Courses
        private CourseController courseController;
        private CourseEnrollmentsController enrollmentsController;

        // --- Exams (NEW)
        private ExamTermsController examController;
        private ExamEnrollmentsController examEnrollmentsController;

        private MessagesController messagesController;

        private StudentViewExams studentViewExams;
        private StudentViewCourses studentViewCourses;
        private StudentViewProfile studentViewProfile;

        private string currentCoursePanel = string.Empty;
        private string currentExamPanel = string.Empty;


        public StudentWindow(Student student, StudentsController studentsController)
        {
            InitializeComponent();
            DataContext = this;

            this.studentsController = studentsController;

            // Controllers
            courseController = new CourseController();
            enrollmentsController = new CourseEnrollmentsController();

            // NEW: exam controllers
            examController = new ExamTermsController();
            examEnrollmentsController = new ExamEnrollmentsController();

            messagesController = new MessagesController();

            // Views
            studentViewExams = new StudentViewExams(
                examController,
                examEnrollmentsController,
                this.studentsController,
                ExamLanguageCombo,
                ExamLevelCombo,
                ExamFromDate,
                ExamToDate,
                ExamsGrid,
                student.Email
            );

            studentViewCourses = new StudentViewCourses(
                courseController,
                enrollmentsController,
                CourseLanguageCombo,
                CourseLevelCombo,
                CourseOnlineCombo,
                CourseWeeksBox,
                CourseFromDate,
                CourseToDate,
                CoursesGrid,
                student.Email,
                this.studentsController,
                messagesController
            );

            RateWindow? rateWindow = studentViewCourses.CheckIfActiveCourseFinished();

            Student = new StudentDTO(student);
            UpdateStudent = new StudentDTO(student);

            studentViewProfile = new StudentViewProfile(
                Student,
                this.studentsController,
                courseController,
                enrollmentsController,
                examEnrollmentsController,
                messagesController,
                FirstNameBox,
                LastNameBox,
                MaleRadioButton,
                FemaleRadioButton,
                BirthDatePicker,
                PhoneBox,
                EmailBox,
                PasswordTextBox,
                EducationComboBox
            );

            studentViewProfile.SetLabels(
                NameLabel,
                SurnameLabel,
                GenderLabel,
                BirthDateLabel,
                PhoneLabel,
                EmailLabel,
                EducationLabel,
                CurrentCourseLabel,
                PenaltyPointsLabel,
                JoinedDateLabel
            );

            InformationUpdate();

            if (rateWindow != null)
            {
                rateWindow.Show();
                rateWindow.Topmost = true;
            }
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
            if (Student.ActiveCourseId != -1)
            {
                MessageBox.Show(
                    "You cannot edit your profile while enrolled in a course. Please finish or leave your current course before making changes.",
                    "Edit Profile",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (Student.ActiveExamTermId != -1)
            {
                MessageBox.Show("You cannot edit your profile after you have applied for an exam.",
                    "Edit Profile", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            studentViewProfile.SetStudentData();

            MainPanel.Visibility = Visibility.Collapsed;
            EditPanel.Visibility = Visibility.Visible;
        }

        private void SaveEditChanges_Click(object sender, RoutedEventArgs e)
        {
            if (studentViewProfile.SaveEditChanges(UpdateStudent))
            {
                Student student = UpdateStudent.ToStudent();
                Student = new StudentDTO(student);
                CloseEditPanel_Click(sender, e);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (studentViewProfile.DeleteProfile())
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
            currentExamPanel = "show all";
            ExamPanelButton.Content = "Enroll";
            studentViewExams.ShowAllExams();
            MainPanel.Visibility = Visibility.Collapsed;
            ExamsPanel.Visibility = Visibility.Visible;
        }

        private void CloseExamsPanel_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Visibility = Visibility.Visible;
            ExamsPanel.Visibility = Visibility.Collapsed;
            currentExamPanel = string.Empty;
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
            if (studentViewExams == null) return;
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
            CoursePanelButton.Content = "Cancel";
            studentViewCourses.ShowRequestedCourses();
            OpenCoursesPanel_Click(sender, e);
        }

        private void ShowActiveCourse_Click(object sender, RoutedEventArgs e)
        {
            currentCoursePanel = "show active";
            CoursePanelButton.Content = "Unenroll";
            studentViewCourses.ShowActiveCourse();
            OpenCoursesPanel_Click(sender, e);
        }

        private void ShowFinishedCourses_Click(object sender, RoutedEventArgs e)
        {
            currentCoursePanel = string.Empty;
            studentViewCourses.ShowFinishedCourses();
        }

        private void CoursesSearch_Click(object sender, RoutedEventArgs e)
        {
            studentViewCourses.SearchCourses();
        }

        private void CoursePanelButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentCoursePanel == "show all")
            {
                EnrollRequestCourse_Click();
            }
            else if (currentCoursePanel == "show requested")
            {
                CancelRequestCourse_Click();
            }
            else if (currentCoursePanel == "show active")
            {
                UnenrollRequestCourse_Click();
            }
        }

        private void EnrollRequestCourse_Click()
        {
            studentViewCourses.RequestForCourse();
        }

        private void CancelRequestCourse_Click()
        {
            studentViewCourses.CancelRequestForCourse();
        }

        private void UnenrollRequestCourse_Click()
        {
            studentViewCourses.UnenrollRequestCourse();
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


        private void ShowRequestedExams_Click(object sender, RoutedEventArgs e)
        {
            currentExamPanel = "show requested";
            ExamPanelButton.Content = "Cancel";
            studentViewExams.ShowRequestedExams();
            MainPanel.Visibility = Visibility.Collapsed;
            ExamsPanel.Visibility = Visibility.Visible;
        }

        private void ShowActiveExam_Click(object sender, RoutedEventArgs e)
        {
            currentExamPanel = "show active";
            ExamPanelButton.Content = "Cancel"; // ili "Unenroll" po želji
            studentViewExams.ShowActiveExam();
            MainPanel.Visibility = Visibility.Collapsed;
            ExamsPanel.Visibility = Visibility.Visible;
        }

        private void ShowFinishedExams_Click(object sender, RoutedEventArgs e)
        {
            studentViewExams.ShowFinishedExams();
        }

        private void ExamPanelButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentExamPanel == "show all")
            {
                studentViewExams.RequestForExam();       // prijavi selektovani termin
            }
            else if (currentExamPanel == "show requested" || currentExamPanel == "show active")
            {
                studentViewExams.CancelRequestForExam(); // otkaži selektovani termin (radi i za active/requested)
            }
        }


        private void OpenInboxPanel_Click(object sender, RoutedEventArgs e)
        {
            MessagesInboxWindow messagesInboxWindow = new MessagesInboxWindow(Student.Email, messagesController);
            messagesInboxWindow.Show();
        }
    }
}
