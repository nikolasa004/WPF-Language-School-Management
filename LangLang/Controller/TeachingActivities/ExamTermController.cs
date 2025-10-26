using System;
using System.Collections.Generic;
using System.Windows;
using LangLang.Model.DAO.TeachingActivity;
using LangLang.Model.TeachingActivities;
using LangLang.Service.TeachingActivities;
using LangLang.SQL;
using LangLang.SQL.Repositories;

namespace LangLang.Controller.TeachingActivities
{
    public class ExamTermsController
    {
        private readonly ExamTermsDAO _dao;
        private readonly ExamTermRepository _sqlRepo;
        private readonly ExamTermService _service;

        public ExamTermsController()
        {
            _dao = new ExamTermsDAO();
            _sqlRepo = new ExamTermRepository();
            _service = new ExamTermService();

            using var context = new LangLangContext();
            context.Database.EnsureCreated();
            SyncJsonToSql();
        }

        private void SyncJsonToSql()
        {
            try
            {
                var sqlTerms = _sqlRepo.GetAll();
                if (sqlTerms?.Count > 0) return;

                var jsonTerms = _dao.GetAllExamTerms();
                if (jsonTerms == null) return;

                foreach (var term in jsonTerms)
                    _sqlRepo.Add(term);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Warning: Could not sync exam terms. Error: {ex.Message}",
                    "Sync Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public List<ExamTerm> GetMyExamTerms(string professorId)
            => _dao.GetExamTermsByProfessor(professorId);

        public List<ExamTerm> SearchExamTerms(List<ExamTerm> terms, Model.Enums.Language? language,
            Model.Enums.LanguageLevel? level, DateTime? fromDate, DateTime? toDate)
            => _service.Search(terms, language, level, fromDate, toDate);

        public ExamTerm Create(ExamTerm term)
        {
            var professorExams = _dao.GetExamTermsByProfessor(term.ProfessorId);
            _service.ValidateCreate(term, professorExams);
            return _dao.AddExamTerm(term);
        }

        public ExamTerm? Update(ExamTerm term)
        {
            var existing = GetExisting(term.Id);
            var professorExams = _dao.GetExamTermsByProfessor(term.ProfessorId);
            _service.ValidateUpdate(term, professorExams);
            return _dao.UpdateExamTerm(term);
        }

        public ExamTerm? Delete(int id)
        {
            var existing = GetExisting(id);
            _service.ValidateDelete(existing);
            return _dao.RemoveExamTerm(id);
        }

        public ExamTerm? ForceDelete(int id)
            => _dao.RemoveExamTerm(id);

        public ExamTerm? LockExamTerm(int id)
        {
            var exam = GetExisting(id);
            exam.IsLocked = true;
            return _dao.UpdateExamTerm(exam);
        }

        public ExamTerm? UnlockExamTerm(int id)
        {
            var exam = GetExisting(id);
            exam.IsLocked = false;
            return _dao.UpdateExamTerm(exam);
        }

        public ExamTerm? ArchiveExamTerm(int id)
        {
            var exam = GetExisting(id);
            exam.IsArchived = true;
            return _dao.UpdateExamTerm(exam);
        }

        public ExamTerm? GetById(int id)
            => _dao.GetExamTermById(id);

        public List<ExamTerm> GetAll()
            => _dao.GetAllExamTerms();

        private ExamTerm GetExisting(int id)
            => _dao.GetExamTermById(id) ?? throw new InvalidOperationException("Exam term not found.");
    }
}