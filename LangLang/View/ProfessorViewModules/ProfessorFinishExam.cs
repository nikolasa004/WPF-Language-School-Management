using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LangLang.Controller.TeachingActivities;
using LangLang.DTO;

namespace LangLang.View.ProfessorViewModules
{
    internal class ProfessorFinishExam
    {
        private readonly ObservableCollection<ExamResultDTO> _examResults = new();
        private readonly ExamResultsController _examResultsController;

        public ProfessorFinishExam(ExamTermDTO exam, List<StudentDTO> students, DataGrid finishExamGrid, ExamResultsController examResultsController, ExamTermsController examTermsController)
        {
            {
                FinishExamGrid = finishExamGrid;
                _examResultsController = examResultsController;

                _examResults = new ObservableCollection<ExamResultDTO>(
               students.Select(s => new ExamResultDTO
               {Student = s, Exam = exam, ReadingPoints = 0, WritingPoints = 0, ListeningPoints = 0, SpeakingPoints = 0})
                );

                FinishExamGrid.ItemsSource = _examResults;
                examTermsController.UnlockExamTerm(exam.Id);
            }
        }

        public DataGrid FinishExamGrid { get; }

        public void SaveResults()
        {
            try
            {
                foreach (var result in _examResults)
                {
                    _examResultsController.Save(result);
                }

                MessageBox.Show("All results have been saved successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save exam results: {ex.Message}");
            }
        }
    }
}
