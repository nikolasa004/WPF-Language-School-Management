using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;
using System.Windows.Controls;
using LangLang.Controller.TeachingActivities;
using LangLang.Controller.Users;
using LangLang.DTO;

internal class ProfessorStartExam
{
    private readonly ObservableCollection<StudentDTO> _students;
    private readonly int _examTermId;
    private readonly StudentsController _studentsController;
    private readonly ExamTermsController _examController;

    public ProfessorStartExam(ExamTermDTO examTerm, List<StudentDTO> allStudents, DataGrid examStudentsGrid,
        StudentsController studentsController, ExamTermsController examController)
    {
        ExamStudentsGrid = examStudentsGrid;
        _examTermId = examTerm.Id;
        _studentsController = studentsController;
        _examController = examController;

        var eligibleStudents = allStudents.Where(s => s.RequestedExamTerms.Contains(examTerm.Id) && !s.IsExamApplicationLocked).ToList();

        _students = new ObservableCollection<StudentDTO>(eligibleStudents);
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

        _examController.LockExamTerm(_examTermId);

        MessageBox.Show($"Exam confirmed for {_students.Count} students.");
        return true;
    }
}
