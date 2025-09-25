using System.Collections.Generic;
using LangLang.Controller.Users;
using LangLang.DTO;
using LangLang.Model.DAO.TeachingActivity;
using LangLang.Model.TeachingActivities;

namespace LangLang.Controller.TeachingActivities
{
    public class ExamResultsController
    {
        private readonly ExamResultsDAO _examResultsDAO;

        public ExamResultsController()
        {
            _examResultsDAO = new ExamResultsDAO();
        }

        public void Save(ExamResultDTO dto)
        {
            var result = dto.ToExamResult();
            _examResultsDAO.AddExamResult(result);
        }
        public ExamResult? GetRawResult(int examId, string studentId)
        => _examResultsDAO.GetByExamAndStudent(examId, studentId);

        public List<ExamResult> GetAll() => _examResultsDAO.GetAllExamResults();
    }
}
