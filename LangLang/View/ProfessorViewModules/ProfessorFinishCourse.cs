using LangLang.Controller.Users;
using LangLang.DTO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Linq;
using LangLang.Controller.TeachingActivities;
using System.Windows;

namespace LangLang.View.ProfessorViewModules
{
    internal class ProfessorFinishCourse
    {
        private readonly StudentsController _studentController;
        private List<StudentDTO> _allStudents;
        private readonly ObservableCollection<StudentDTO> _studentsObservable = new();

        private DataGrid StudentsGrid;
        private int _currentCourseId;

        public ProfessorFinishCourse(List<StudentDTO> sharedStudents, DataGrid studentsGrid, int currentCourseId)
        {
            _studentController = new StudentsController();
            _allStudents = sharedStudents ?? new List<StudentDTO>();
            StudentsGrid = studentsGrid;
            _currentCourseId = currentCourseId;

            StudentsGrid.ItemsSource = _studentsObservable;
            LoadStudents();
        }

        private void LoadStudents()
        {
            _studentsObservable.Clear();
            foreach (var student in _allStudents.Where(s => s.ActiveCourseId == _currentCourseId))
            {
                _studentsObservable.Add(student);
            }
        }

        public void RefreshStudents(List<StudentDTO> updatedStudents)
        {
            _allStudents = updatedStudents ?? new List<StudentDTO>();
            LoadStudents();
        }

        public bool FinishCourseAndGradeStudents(int courseId, CourseController courseController, DataGrid grid, StudentsController studentController)
        {
            foreach (var item in grid.Items)
            {
                if (item is StudentDTO student)
                {
              
                    if (!int.TryParse(student.TempKnowledgeGrade, out int knowledge) || knowledge < 1 || knowledge > 10)
                    {
                        MessageBox.Show($"Invalid knowledge grade for {student.FirstName} {student.LastName}. Must be 1-10.");
                        return false;
                    }

                    if (!int.TryParse(student.TempActivityGrade, out int activity) || activity < 1 || activity > 10)
                    {
                        MessageBox.Show($"Invalid activity grade for {student.FirstName} {student.LastName}. Must be 1-10.");
                        return false;
                    }

               
                    courseController.AddGrade(courseId, student.Email, knowledge, activity);

               
                    student.ActiveCourseId = -1;
                    studentController.RemoveFromCourse(student.Email, courseId);
                }
            }

            return true;
        }

    }
}