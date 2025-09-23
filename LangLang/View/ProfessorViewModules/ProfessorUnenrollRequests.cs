using LangLang.Controller.Messages;
using LangLang.Controller.TeachingActivities;
using LangLang.Controller.Users;
using LangLang.Model.Users;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LangLang.View.ProfessorViewModules
{
    internal class ProfessorUnenrollRequests
    {
        internal class UnenrollRequestDisplayDTO
        {
            public int MessageId { get; set; }
            public string StudentEmail { get; set; }
            public string StudentName { get; set; }
            public int CourseId { get; set; }
            public string CourseName { get; set; }
            public string Body { get; set; }
            public string Date { get; set; }
        }

        private readonly StudentsController _studentController;
        private readonly CourseController _courseController;
        private readonly MessagesController _messagesController;

        private readonly ObservableCollection<UnenrollRequestDisplayDTO> _requestsObservable = new();
        private readonly DataGrid _requestsGrid;

        public ProfessorUnenrollRequests(
            DataGrid requestsGrid,
            StudentsController studentController,
            CourseController courseController,
            MessagesController messagesController)
        {
            _requestsGrid = requestsGrid ?? throw new ArgumentNullException(nameof(requestsGrid));
            _studentController = studentController ?? throw new ArgumentNullException(nameof(studentController));
            _courseController = courseController ?? throw new ArgumentNullException(nameof(courseController));
            _messagesController = messagesController ?? throw new ArgumentNullException(nameof(messagesController));

            _requestsGrid.ItemsSource = _requestsObservable;
            LoadRequests();
        }

        private void LoadRequests()
        {
            _requestsObservable.Clear();

            var allMessages = _messagesController.GetAllProfessorMessages(); 
            foreach (var msg in allMessages)
            {
                if (msg.Title != "Unenrollment Request") continue;

                var student = _studentController.GetStudentById(msg.SenderId);
                if (student == null || student.ActiveCourseId == -1)  
                    continue;

                var course = _courseController.GetById(student.ActiveCourseId);
                if (course == null) continue;

                _requestsObservable.Add(new UnenrollRequestDisplayDTO
                {
                    MessageId = msg.Id,
                    StudentEmail = student.Email,
                    StudentName = student.FirstName + " " + student.LastName,
                    CourseId = course.Id,
                    CourseName = course.Language + " " + course.Level,
                    Body = msg.Body,
                    Date = msg.Date
                });
            }
        }

        public void ApproveUnenroll_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is UnenrollRequestDisplayDTO request)
            {
                var confirm = MessageBox.Show(
                    $"Are you sure you want to approve unenrollment for {request.StudentName} from {request.CourseName}?",
                    "Confirm Action",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirm != MessageBoxResult.Yes) return;

        
                _studentController.RemoveFromCourse(request.StudentEmail, request.CourseId);
                _messagesController.ProfessorDelete(request.MessageId);
                _requestsObservable.Remove(request);

                MessageBox.Show($"{request.StudentEmail} has been unenrolled from {request.CourseName}.");
            }
        }

        public void RejectUnenroll_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is UnenrollRequestDisplayDTO request)
            {
                var confirm = MessageBox.Show(
                    $"Are you sure you want to reject unenrollment for {request.StudentName} from {request.CourseName}? This will add 1 penalty point.",
                    "Confirm Action",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (confirm != MessageBoxResult.Yes) return;

                var student = _studentController.GetAllStudents()
                                                .FirstOrDefault(s => s.Email == request.StudentEmail);
                if (student == null) return;

                _studentController.AddPenaltyPoint(student.Email);

                _studentController.RemoveFromCourse(request.StudentEmail, request.CourseId);
                _messagesController.ProfessorDelete(request.MessageId);
                _requestsObservable.Remove(request);

                MessageBox.Show($"Unenroll request from {student.FirstName} {student.LastName} rejected. 1 penalty point added.");
            }
        }

    }
}