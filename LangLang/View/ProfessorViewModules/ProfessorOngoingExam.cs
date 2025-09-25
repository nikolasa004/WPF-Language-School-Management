using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LangLang.Controller.TeachingActivities;
using LangLang.Controller.Users;
using LangLang.DTO;

namespace LangLang.View.ProfessorViewModules
{
    internal class ProfessorOngoingExam
    {
        private readonly ObservableCollection<StudentDTO> _students;
        private readonly StudentsController _studentsController;
        private readonly int _examTermId;

        public ProfessorOngoingExam(ExamTermDTO examTerm, List<StudentDTO> allStudents, DataGrid ongoingExamGrid, StudentsController studentsController)
        {
            OngoingExamGrid = ongoingExamGrid;
            _studentsController = studentsController;
            _examTermId = examTerm.Id;

            var activeStudents = allStudents.Where(s => s.ActiveExamTermId == _examTermId).ToList();

            _students = new ObservableCollection<StudentDTO>(activeStudents);
            OngoingExamGrid.ItemsSource = _students;
        }

        public DataGrid OngoingExamGrid { get; }

        public void BanStudent(StudentDTO student)
        {
            if (student == null)
            {
                MessageBox.Show("No student selected.");
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to ban {student.FirstName} {student.LastName} ({student.Email}) forever?",
                "Confirm Ban",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                _studentsController.Delete(student.Email);
                _students.Remove(student);

                MessageBox.Show($"Student {student.FirstName} {student.LastName} has been banned.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to ban student: {ex.Message}");
            }
        }
    }
}
