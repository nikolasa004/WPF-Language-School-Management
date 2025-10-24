using LangLang.Controller.Messages;
using LangLang.Controller.TeachingActivities;
using LangLang.Controller.Users;
using LangLang.DTO;
using LangLang.Model.Messages;
using LangLang.Model.TeachingActivities;
using LangLang.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace LangLang.View.ProfessorViewModules
{
    internal class ProfessorOngoingCourses
    {
        private readonly StudentsController _studentController;
        private readonly MessagesController _messagesController;
        private readonly PenaltyPointsController _penaltyPointsController = new();

        private List<StudentDTO> _allStudents;
        private readonly ObservableCollection<StudentDTO> _studentsObservable = new();

        private DataGrid StudentsGrid;
        private int _currentCourseId;
        private CourseDTO _currentCourseDTO;

        public ProfessorOngoingCourses(
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
            {
                if (student.ActiveCourseId == _currentCourseId)
                    _studentsObservable.Add(student);
            }
        }

        public void ExpelStudent_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is StudentDTO student)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to expel {student.FirstName} {student.LastName} from this course?\n" +
                    $"They will also receive 1 penalty point.",
                    "Confirm Expel",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    
                    student.ActiveCourseId = -1;
                    student.PenaltyPoints += 1;

                    _studentController.RemoveFromCourse(student.Email, _currentCourseId);
                    _studentController.AddPenaltyPoint(student.Email); 

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
                        $"You have been expelled from {_currentCourseDTO?.Language} {_currentCourseDTO?.Level}",
                        $"You were expelled from the course and received 1 penalty point.\n\n{courseInfo}",
                        student.Email,
                        _messagesController,
                        "student"
                    );
                    messageWindow.Show();
                }
            }
        }

        public void AddPenalty_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is StudentDTO student)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to give 1 penalty point to {student.FirstName} {student.LastName}?",
                    "Confirm Penalty",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
      
                    student.PenaltyPoints += 1;
                    _studentController.AddPenaltyPoint(student.Email);

                    string courseInfo = _currentCourseDTO != null
                        ? $"Course: {_currentCourseDTO.Language} {_currentCourseDTO.Level}\n" +
                          $"Start Date: {_currentCourseDTO.Date}\n" +
                          $"Time: {_currentCourseDTO.StartAt}\n" +
                          $"Duration: {_currentCourseDTO.DurationWeeks} weeks\n" +
                          $"Is Live: {(_currentCourseDTO.IsLive ? "Yes" : "No")}"
                        : "Course information unavailable.";

                    MessageSendWindow messageWindow = new MessageSendWindow(
                        _currentCourseDTO?.ProfessorId ?? "",
                        $"You have received a penalty point in {_currentCourseDTO?.Language} {_currentCourseDTO?.Level}",
                        $"You have received 1 penalty point.\n\n{courseInfo}",
                        student.Email,
                        _messagesController,
                        "student"
                    );
                    messageWindow.Show();

                    PenaltyPoint penaltyPoint = new PenaltyPoint(-1, student.Email, _currentCourseDTO!.Id);
                    _penaltyPointsController.AddPenaltyPoint(penaltyPoint);
                    _penaltyPointsController.AddPenaltyPointSQL(penaltyPoint);

                    RefreshStudents(_allStudents);
                }
            }
        }

        public void ViewUnenrollRequest_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is StudentDTO student)
            {
                MessageBox.Show($"ViewUnenrollRequest_Click for {student.FirstName} {student.LastName} (to be implemented).");
            }
        }

        public void RefreshStudents(List<StudentDTO> updatedStudents)
        {
            _allStudents = updatedStudents ?? new List<StudentDTO>();
            LoadStudents();
        }


    }
}
