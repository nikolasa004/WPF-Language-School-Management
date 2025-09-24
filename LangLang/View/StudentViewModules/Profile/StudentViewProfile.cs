using LangLang.Controller.Messages;
using LangLang.Controller.Requests;
using LangLang.Controller.TeachingActivities;
using LangLang.Controller.Users;
using LangLang.DTO;
using LangLang.Model.TeachingActivities;
using LangLang.Utils;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace LangLang.View.StudentViewModules.Profile
{
    class StudentViewProfile
    {
        private readonly StudentsController studentsController;
        private readonly CourseController courseController;
        private readonly CourseEnrollmentsController enrollmentsController;
        private readonly MessagesController messagesController;
        private readonly ExamEnrollmentsController examEnrollmentsController;

        private StudentDTO Student;

        private Label NameLabel = null!;
        private Label SurnameLabel = null!;
        private Label GenderLabel = null!;
        private Label BirthDateLabel = null!;
        private Label PhoneLabel = null!;
        private Label EmailLabel = null!;
        private Label EducationLabel = null!;
        private Label CurrentCourseLabel = null!;
        private Label PenaltyPointsLabel = null!;
        private Label JoinedDateLabel = null!;

        public StudentViewProfile(StudentDTO student, StudentsController studentsController, CourseController courseController, CourseEnrollmentsController courseEnrollmentsController, ExamEnrollmentsController examEnrollmentsController, MessagesController messagesController)
        {
            Student = student;
            this.studentsController = studentsController;
            this.courseController = courseController;
            enrollmentsController = courseEnrollmentsController;
            this.examEnrollmentsController = examEnrollmentsController; 
            this.messagesController= messagesController;
        }
        public void SetLabels(ProfileLabelsPackage labelsPackage)
        {
            NameLabel = labelsPackage.NameLabel;
            SurnameLabel =labelsPackage.SurnameLabel;
            GenderLabel = labelsPackage.GenderLabel;
            BirthDateLabel =labelsPackage.BirthDateLabel;
            PhoneLabel = labelsPackage.PhoneLabel;
            EmailLabel = labelsPackage.EmailLabel;
            EducationLabel = labelsPackage.EducationLabel;
            CurrentCourseLabel = labelsPackage.CurrentCourseLabel;
            PenaltyPointsLabel = labelsPackage.PenaltyPointsLabel;
            JoinedDateLabel = labelsPackage.JoinedDateLabel;
        }

        public void InformationUpdate(bool fromEdit=false, StudentDTO? updatedStudent=null)
        {
            if (fromEdit && updatedStudent != null)
                Student = updatedStudent;
            NameLabel.Content = "Name: " + Student.FirstName;
            SurnameLabel.Content = "Surname: " + Student.LastName;
            GenderLabel.Content = "Gender: " + Student.Gender;
            DateTime birthDate = DateTime.ParseExact(Student.DateOfBirth, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            string birthDateString = birthDate.ToString("dd.MM.yyyy");
            BirthDateLabel.Content = "Birth Date: " + birthDateString;
            PhoneLabel.Content = "Phone: " + Student.PhoneNumber;
            EmailLabel.Content = "Email: " + Student.Email;
            EducationLabel.Content = "Education: " + Student.EducationLevel.ToString();
            Course? course = courseController.GetById(Student.ActiveCourseId);
            string courseName = course != null ? course.Language+"-"+course.Level : "No active course";
            CurrentCourseLabel.Content = "Current Course: " + courseName;
            PenaltyPointsLabel.Content = "Penalty Points: " + Student.PenaltyPoints.ToString();
            DateTime joinedDate = DateTime.ParseExact(Student.DateOfRegistration, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            string joinedDateString = joinedDate.ToString("dd.MM.yyyy");
            JoinedDateLabel.Content = "Joined Date: " + joinedDateString;
        }
        public bool DeleteProfile()
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete your account? All your data will be lost.", "Delete Account", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                studentsController.Delete(Student.Email);
                messagesController.StudentDeleteByStudent(Student.Email);
                enrollmentsController.DeleteByStudent(Student.Email);
                examEnrollmentsController.DeleteByStudent(Student.Email);
                MessageBox.Show("Your account has been deleted successfully.");
                return true;
            }
            return false;
        }
    }
}