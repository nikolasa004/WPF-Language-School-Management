using System.Collections.Generic;
using System.Linq;
using System.Windows;
using LangLang.DTO;
using LangLang.Model.DAO.TeachingActivity;
using LangLang.Model.TeachingActivities;
using LangLang.SQL;
using LangLang.SQL.Repositories;

namespace LangLang.Controller.TeachingActivities
{
    public class ExamResultsController
    {
        private readonly ExamResultsDAO _examResultsDAO;
        private readonly ExamResultRepository _sqlRepo;

        public ExamResultsController()
        {
            _examResultsDAO = new ExamResultsDAO();
            _sqlRepo = new ExamResultRepository();

            using var context = new LangLangContext();
            context.Database.EnsureCreated();

            SyncJsonToSqlIfEmpty();

        }
        private void SyncJsonToSqlIfEmpty()
        {
            try
            {
                var sqlResults = _sqlRepo.GetAll();

                if (sqlResults == null || sqlResults.Count == 0)
                {
                    var jsonResults = _examResultsDAO.GetAllExamResults();

                    if (jsonResults != null && jsonResults.Count > 0)
                    {
                        foreach (var result in jsonResults)
                        {
                            _sqlRepo.Add(result);  
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Warning: Could not sync data from JSON to database. Error: {ex.Message}",
                    "Sync Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void Save(ExamResultDTO dto)
        {
            var result = dto.ToExamResult();
            _sqlRepo.Add(result);
            _examResultsDAO.AddExamResult(result);
        }
        public ExamResult? GetRawResult(int examId, string studentId)
            => _sqlRepo.GetByExamAndStudent(examId, studentId);
        public List<ExamResult> GetAll()
            => _sqlRepo.GetAll();
    }
}
