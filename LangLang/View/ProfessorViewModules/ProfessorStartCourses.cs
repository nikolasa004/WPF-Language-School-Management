using LangLang.Controller.Messages;
using LangLang.Controller.TeachingActivities;
using LangLang.Controller.Users;
using LangLang.DTO;
using LangLang.Model.Messages;
using LangLang.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LangLang.RoleWindows.ProfessorViewModules
{
    internal class ProfessorStartCourses
    {
     

        private readonly StudentsController _studentController;
        private readonly MessagesController _messagesController;

        private List<StudentDTO> _allStudents;
        private readonly ObservableCollection<StudentDTO> _studentsObservable = new();

        private DataGrid StudentsGrid;
        private int _currentCourseId;
        private CourseDTO _currentCourseDTO; 


        public ProfessorStartCourses(
     List<StudentDTO> sharedStudents,
     DataGrid studentsGrid,
     int currentCourseId,
     CourseDTO currentCourseDTO,
     MessagesController messagesController)   
        {
            _studentController = new StudentsController();
            _messagesController = messagesController; 
            _allStudents = sharedStudents ?? new List<StudentDTO>();
            StudentsGrid = studentsGrid;
            _currentCourseId = currentCourseId;
            _currentCourseDTO = currentCourseDTO;

            StudentsGrid.ItemsSource = _studentsObservable;
            LoadStudents();
        }
        private void LoadStudents()
        {
            _studentsObservable.Clear();
            foreach (var student in _allStudents)
                _studentsObservable.Add(student);
        }

        public void RejectStudent_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is StudentDTO student)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to reject {student.FirstName} {student.LastName} for this course?",
                    "Confirm Reject",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    StudentsController studentsController = new StudentsController();
                    studentsController.RemoveCourseRequest(student.Email, _currentCourseId);

                    _studentsObservable.Remove(student);

                    string courseInfo = _currentCourseDTO != null
                        ? $"Course: {_currentCourseDTO.Language} {_currentCourseDTO.Level}\n" +
                          $"Start Date: {_currentCourseDTO.Date}\n" +
                          $"Time: {_currentCourseDTO.StartAt}\n" +
                          $"Duration: {_currentCourseDTO.DurationWeeks} weeks\n" +
                          $"Is Live: {(_currentCourseDTO.IsLive ? "Yes" : "No")}"
                        : "Course information unavailable.";

                    MessageSendWindow messageWindow = new MessageSendWindow(
              _currentCourseDTO?.ProfessorId ?? "",  
              $"Your request for {_currentCourseDTO?.Language} {_currentCourseDTO?.Level} was rejected",
              "Please provide a reason for rejection:",
              student.Email,                          
              _messagesController,
              "student"                               
          );
                    messageWindow.Show();
                }
            }
        }


        public void StartCourse_Click(object sender, RoutedEventArgs e)
        {
            string courseInfo = _currentCourseDTO != null
                ? $"Course: {_currentCourseDTO.Language} {_currentCourseDTO.Level}\n" +
                  $"Start Date: {_currentCourseDTO.Date}\n" +
                  $"Time: {_currentCourseDTO.StartAt}\n" +
                  $"Duration: {_currentCourseDTO.DurationWeeks} weeks\n" +
                  $"Is Live: {(_currentCourseDTO.IsLive ? "Yes" : "No")}"
                : "Course information unavailable.";

            foreach (var student in _allStudents)
            {
                if (student.RequestedCourses.Contains(_currentCourseId))
                {
                    if (student.ActiveCourseId != -1)
                        continue;

                    student.RequestedCourses.Remove(_currentCourseId);
                    student.ActiveCourseId = _currentCourseId;

                    _studentController.StartCourse(student.Email, _currentCourseId);

                    Message msg = new Message
                    {
                        RecipientId = student.Email,
                        SenderId = _currentCourseDTO.ProfessorId,
                        Title = $"You have been enrolled in {_currentCourseDTO.Language} {_currentCourseDTO.Level}",
                        Body = $"You have been successfully enrolled in the course.\n\n{courseInfo}"
                    };
                    _messagesController.StudentAdd(msg); 
                }
            }

            LoadStudents();

            MessageBox.Show("All eligible students have been enrolled in the course and notified.",
                            "Course Started", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        public void RefreshStudents(List<StudentDTO> updatedStudents)
        {
            _allStudents = updatedStudents ?? new List<StudentDTO>();
            LoadStudents();
        }

    }
}
