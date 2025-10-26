using LangLang.Model.TeachingActivities;
using System.Collections.Generic;
using System.Linq;

namespace LangLang.SQL.Repositories
{
    public class ExamResultRepository
    {
        public void Add(ExamResult result)
        {
            using (var context = new LangLangContext())
            {
                context.Database.EnsureCreated();
                context.ExamResults.Add(result);
                context.SaveChanges();
            }
        }

        public List<ExamResult> GetAll()
        {
            using var context = new LangLangContext();
            return context.ExamResults.ToList();
        }

        public ExamResult? GetByExamAndStudent(int examId, string studentId)
        {
            using var context = new LangLangContext();
            return context.ExamResults
                .FirstOrDefault(r => r.ExamId == examId && r.StudentId == studentId);
        }
    }
}
