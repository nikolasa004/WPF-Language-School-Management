using System.Windows;
using LangLang.Controller.Messages;
using LangLang.DTO;

internal class ProfessorActionsExam
{
    public ProfessorActionsExam()
    {
    }

    public void StartExam()
    {
        // logic to start exam (notify students, update db, etc.)
        MessageBox.Show("Exam started and students notified.");
    }

    public void OpenExam()
    {
        // todo
        MessageBox.Show("Exam is currently open.");
    }

    public void FinishExam()
    {
        // todo
        MessageBox.Show("Exam is currently finished.");
    }
}
