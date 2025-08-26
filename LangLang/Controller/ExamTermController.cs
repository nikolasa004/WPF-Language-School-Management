using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LangLang.Model;
using LangLang.Model.DAO;
namespace LangLang.Controller
{
    public class ExamTermsController
    {
        private readonly ExamTermsDAO _dao;
        private static readonly CultureInfo Sr = new("sr-RS");

        public ExamTermsController()
        {
            _dao = new ExamTermsDAO();
        }
        public List<ExamTerm> GetMyExamTerms(string professorId)
            => _dao.GetExamTermsByProfessor(professorId);
        public List<ExamTerm> SearchMyExamTerms(
            string professorId,
            LangLang.Model.Enums.Language? language,
            LangLang.Model.Enums.LanguageLevel? level,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var items = _dao.GetExamTermsByProfessor(professorId).AsEnumerable();
            if (language is not null) items = items.Where(t => t.Language == language);
            if (level is not null) items = items.Where(t => t.Level == level);
            if (fromDate.HasValue ^ toDate.HasValue)
            {
                var exact = (fromDate ?? toDate)!.Value.Date;
                fromDate = exact;
                toDate = exact;
            }
            if (fromDate.HasValue && toDate.HasValue)
            {
                var from = fromDate.Value.Date;
                var to = toDate.Value.Date;
                if (from > to) (from, to) = (to, from); 
                items = items.Where(t =>
                {
                    var start = TryGetStartDateTime(t);
                    if (!start.HasValue) return false;
                    var d = start.Value.Date;
                    return d >= from && d <= to;
                });
            }
            return items.OrderBy(t => TryGetStartDateTime(t)).ToList();
        }
        public ExamTerm Create(ExamTerm term)
        {
            term.IsLive = true;
            term.Duration = 240;
            EnsureValidBasics(term);
            EnsureNoOverlap(term, ignoreId: null);
            return _dao.AddExamTerm(term);
        }
        public ExamTerm? Update(ExamTerm term)
        {
            EnsureCanModify(term.Id);
            term.IsLive = true;
            term.Duration = 240;
            EnsureValidBasics(term);
            EnsureNoOverlap(term, ignoreId: term.Id);
            return _dao.UpdateExamTerm(term);
        }
        public ExamTerm? Delete(int id)
        {
            EnsureCanModify(id);
            return _dao.RemoveExamTerm(id);
        }
        public ExamTerm? GetById(int id) => _dao.GetExamTermById(id);
        private static void EnsureValidBasics(ExamTerm t)
        {
            if (t.MaxAttendees is null || t.MaxAttendees <= 0)
                throw new ArgumentException("MaxAttendees must be greater than 0.");
            if (string.IsNullOrWhiteSpace(t.Date) || !DateTime.TryParseExact(t.Date, "dd.MM.yyyy", Sr,
                    DateTimeStyles.None, out _))
                throw new ArgumentException("Invalid exam date (dd.MM.yyyy).");
            if (string.IsNullOrWhiteSpace(t.StartAt) ||
                !TimeSpan.TryParseExact(t.StartAt, "hh\\:mm", CultureInfo.InvariantCulture, out _))
                throw new ArgumentException("Invalid start time (HH:mm).");
        }
        private void EnsureCanModify(int examId)
        {
            var current = _dao.GetExamTermById(examId)
                          ?? throw new InvalidOperationException("Exam term not found.");
            var start = TryGetStartDateTime(current)
                        ?? throw new InvalidOperationException("Exam term has invalid date/time.");
            if ((start - DateTime.Now).TotalDays < 14)
                throw new InvalidOperationException("Cannot modify or cancel an exam within 14 days of start.");
        }
        private void EnsureNoOverlap(ExamTerm candidate, int? ignoreId)
        {
            var start = TryGetStartDateTime(candidate)
                        ?? throw new ArgumentException("Invalid date/time.");
            var end = start.AddMinutes(240); 
            var mine = _dao.GetExamTermsByProfessor(candidate.ProfessorId);
            foreach (var other in mine)
            {
                if (ignoreId.HasValue && other.Id == ignoreId.Value) continue;
                var otherStart = TryGetStartDateTime(other);
                if (!otherStart.HasValue) continue;
                var otherEnd = otherStart.Value.AddMinutes(240);
                bool overlap = start < otherEnd && otherStart.Value < end;
                if (overlap)
                    throw new InvalidOperationException("Professor already has an exam overlapping in that time window.");
            }
        }
        private static DateTime? TryGetStartDateTime(ExamTerm t)
        {
            if (!DateTime.TryParseExact(t.Date ?? "", "dd.MM.yyyy", Sr, DateTimeStyles.None, out var d))
                return null;

            if (!TimeSpan.TryParseExact(t.StartAt ?? "", "hh\\:mm", CultureInfo.InvariantCulture, out var ts))
                return null;
            return d.Date.Add(ts);
        }
    }
}
