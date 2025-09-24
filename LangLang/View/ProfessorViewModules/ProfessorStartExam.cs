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
    internal class ProfessorStartExam
    {
        private readonly ObservableCollection<StudentDTO> _students = new();
        private readonly int _examTermId;
        private readonly StudentsController _studentsController;
        private readonly ExamTermsController _examControler;

        public ProfessorStartExam(List<StudentDTO> students, DataGrid examStudentsGrid, int examTermId, StudentsController studentsController, ExamTermsController examTermsController)
        {
            ExamStudentsGrid = examStudentsGrid;
            _examTermId = examTermId;
            _studentsController = studentsController;
            _examControler = examTermsController; 

            _students = new ObservableCollection<StudentDTO>(students);
            ExamStudentsGrid.ItemsSource = _students;
        }

        public DataGrid ExamStudentsGrid { get; }

        public bool ConfirmExamStart()
        {
            foreach (var student in _students)
            {
                _studentsController.LockExamApplications(student.Email);
                _studentsController.SetActiveExamTerm(student.Email, _examTermId);
            }

            _examControler.LockExamTerm(_examTermId);

            MessageBox.Show($"Exam confirmed for {_students.Count} students.");
            return true;
        }
    }
}