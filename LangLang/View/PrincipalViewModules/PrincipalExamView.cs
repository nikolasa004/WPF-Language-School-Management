using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LangLang.Controller.Messages;
using LangLang.Controller.TeachingActivities;
using LangLang.Controller.Users;
using LangLang.DTO;
using LangLang.Model.Messages;

namespace LangLang.View.PrincipalViewModules
{
    internal class PrincipalExamView
    {
        private readonly ExamTermsController _examsController;

        private readonly ObservableCollection<ExamTermDTO> _examTermsObservable = new();
        private DataGrid ExamTermsGrid;

        public PrincipalExamView(DataGrid examTermsGrid)
        {
            _examsController = new ExamTermsController();
            ExamTermsGrid = examTermsGrid;

            ExamTermsGrid.ItemsSource = _examTermsObservable;
            LoadExamTerms();
        }

        private void LoadExamTerms()
        {
            _examTermsObservable.Clear();

            List<ExamTermDTO> allExams = _examsController.GetAll().Select(e => new ExamTermDTO(e)).ToList();

            foreach (var exam in allExams)
            {
                if (exam.TryGetStart(out var startTime))
                {
                    if (startTime < DateTime.Now && !exam.IsLocked && !exam.IsArchived)
                        _examTermsObservable.Add(exam);
                }
            }
        }

        public void ConfirmResult(int examId)
        {
            var resultsController = new ExamResultsController();
            var messagesController = new MessagesController();
            var studentsController = new StudentsController();
            var exam = _examsController.GetById(examId);

            if (exam != null)
            {
                _examsController.ArchiveExamTerm(examId);
            }

            var allStudents = studentsController.GetAllStudents();

            foreach (var student in allStudents)
            {
                if (student.ActiveExamTermId == examId)
                {
                    var result = resultsController.GetRawResult(examId, student.Email);

                    if (result != null)
                    {
                        Message message = new Message
                        {
                            RecipientId = student.Email,
                            SenderId = "principal@gmail.com",
                            Title = $"Results for Exam {examId}",
                            Body =
                            $"Dear {student.FirstName},\n\n" +
                            $"Here are your results for exam {examId}:\n" +
                            $"- Reading: {result.ReadingPoints}\n" +
                            $"- Writing: {result.WritingPoints}\n" +
                            $"- Listening: {result.ListeningPoints}\n" +
                            $"- Speaking: {result.SpeakingPoints}\n" +
                            $"Total: {result.TotalPoints}\n\n" +
                            $"Status: {(result.Passed ? "PASSED" : "FAILED")}"
                        };


                        messagesController.StudentAdd(message);
                        if(result.Passed)
                        {
                            studentsController.FinishActiveCourse(student.Email, exam.Language, exam.Level);
                        }

                        studentsController.UnlockExamApplications(student.Email);

                    }
                }
            }

            MessageBox.Show("Results confirmed and students notified.");
        }


        public void RefreshExams()
        {
            LoadExamTerms();
        }
    }
}
