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
                    if (startTime < DateTime.Now && !exam.IsLocked)
                        _examTermsObservable.Add(exam);
                }
            }
        }

        public void ConfirmResult(int examId)
        {
            var resultsController = new ExamResultsController();
            var messagesController = new MessagesController();
            var studentsController = new StudentsController();

            var allStudents = studentsController.GetAllStudents();

            foreach (var student in allStudents)
            {
                if (student.ActiveExamTermId == examId)
                {
                    // fetch the result
                    var result = resultsController.GetResultForStudent(examId, student.Email);

                    if (result != null)
                    {
                        // prepare message
                        Message message = new Message
                        {
                            RecipientId = student.Email,
                            SenderId = "principal@gmail.com", 
                            Title = $"Results for Exam {examId}",
                            Body = $"Dear {student.FirstName}, your result for exam {examId} is: {result.TotalPoints}"
                        };

                        messagesController.StudentAdd(message);

                        // reset active exam
                        student.ActiveExamTermId = -1;
                        studentsController.Update(student);
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
