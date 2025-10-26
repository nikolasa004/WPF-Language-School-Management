using LangLang.Model.TeachingActivities;
using System.Collections.Generic;
using System.Linq;

namespace LangLang.SQL.Repositories
{
    public class ExamTermRepository
    {
        public void Add(ExamTerm term)
        {
            using (var context = new LangLangContext())
            {
                context.Database.EnsureCreated();
                context.ExamTerms.Add(term);
                context.SaveChanges();
            }
        }

        public ExamTerm? Update(ExamTerm term)
        {
            using (var context = new LangLangContext())
            {
                context.Database.EnsureCreated();
                context.ExamTerms.Update(term);
                context.SaveChanges();
                return term;
            }
        }

        public ExamTerm? Remove(int id)
        {
            using (var context = new LangLangContext())
            {
                var term = context.ExamTerms.FirstOrDefault(t => t.Id == id);
                if (term != null)
                {
                    context.ExamTerms.Remove(term);
                    context.SaveChanges();
                }
                return term;
            }
        }

        public List<ExamTerm> GetAll()
        {
            using var context = new LangLangContext();
            return context.ExamTerms.ToList();
        }

        public ExamTerm? GetById(int id)
        {
            using var context = new LangLangContext();
            return context.ExamTerms.FirstOrDefault(t => t.Id == id);
        }

        public List<ExamTerm> GetByProfessor(string professorId)
        {
            using var context = new LangLangContext();
            return context.ExamTerms.Where(t => t.ProfessorId == professorId).ToList();
        }
    }
}