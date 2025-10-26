using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LangLang.Model.TeachingActivities;

namespace LangLang.Service.TeachingActivities
{
    public class ExamTermService
    {
        private static readonly CultureInfo SerbianCulture = new("sr-RS");
        private const int ExamDurationMinutes = 240;
        private const int MinDaysBeforeModification = 14;

        public List<ExamTerm> Search(List<ExamTerm> terms, Model.Enums.Language? language,
            Model.Enums.LanguageLevel? level, DateTime? fromDate, DateTime? toDate)
        {
            var filtered = terms.AsEnumerable();

            if (language.HasValue)
                filtered = filtered.Where(t => t.Language == language.Value);

            if (level.HasValue)
                filtered = filtered.Where(t => t.Level == level.Value);

            var (from, to) = NormalizeDateRange(fromDate, toDate);
            if (from.HasValue && to.HasValue)
            {
                filtered = filtered.Where(t =>
                {
                    var start = ParseDateTime(t);
                    return start.HasValue && start.Value.Date >= from && start.Value.Date <= to;
                });
            }

            return filtered.OrderBy(ParseDateTime).ToList();
        }

        public void ValidateCreate(ExamTerm term, List<ExamTerm> professorExams)
        {
            SetDefaults(term);
            ValidateBasics(term);
            ValidateNoConflict(term, professorExams);
        }

        public void ValidateUpdate(ExamTerm term, List<ExamTerm> professorExams)
        {
            SetDefaults(term);
            ValidateBasics(term);
            ValidateTimeWindow(term);
            ValidateNoConflict(term, professorExams, term.Id);
        }

        public void ValidateDelete(ExamTerm term)
        {
            ValidateTimeWindow(term);
        }

        private static void SetDefaults(ExamTerm term)
        {
            term.IsLive = true;
            term.Duration = ExamDurationMinutes;
        }

        private void ValidateBasics(ExamTerm term)
        {
            if (term.MaxAttendees is null or <= 0)
                throw new ArgumentException("Maximum attendees must be greater than 0.");

            if (!DateTime.TryParseExact(term.Date, "dd.MM.yyyy", SerbianCulture, DateTimeStyles.None, out _))
                throw new ArgumentException("Invalid date format. Expected: dd.MM.yyyy");

            if (!TimeSpan.TryParseExact(term.StartAt, "hh\\:mm", CultureInfo.InvariantCulture, out _))
                throw new ArgumentException("Invalid time format. Expected: HH:mm");
        }

        private void ValidateTimeWindow(ExamTerm term)
        {
            var start = ParseDateTime(term) ?? throw new InvalidOperationException("Invalid date/time.");

            if ((start - DateTime.Now).TotalDays < MinDaysBeforeModification)
                throw new InvalidOperationException($"Cannot modify exam within {MinDaysBeforeModification} days.");
        }

        private void ValidateNoConflict(ExamTerm term, List<ExamTerm> professorExams, int? ignoreId = null)
        {
            var start = ParseDateTime(term) ?? throw new ArgumentException("Invalid date/time.");
            var end = start.AddMinutes(ExamDurationMinutes);

            foreach (var other in professorExams)
            {
                if (ignoreId.HasValue && other.Id == ignoreId.Value) continue;

                var otherStart = ParseDateTime(other);
                if (!otherStart.HasValue) continue;

                var otherEnd = otherStart.Value.AddMinutes(ExamDurationMinutes);

                if (start < otherEnd && otherStart < end)
                    throw new InvalidOperationException("Time conflict with another exam.");
            }
        }

        private static (DateTime? from, DateTime? to) NormalizeDateRange(DateTime? from, DateTime? to)
        {
            if (from.HasValue && !to.HasValue) return (from.Value.Date, from.Value.Date);
            if (!from.HasValue && to.HasValue) return (to.Value.Date, to.Value.Date);
            if (!from.HasValue || !to.HasValue) return (null, null);

            var f = from.Value.Date;
            var t = to.Value.Date;
            return f > t ? (t, f) : (f, t);
        }

        private static DateTime? ParseDateTime(ExamTerm term)
        {
            if (!DateTime.TryParseExact(term.Date, "dd.MM.yyyy", SerbianCulture, DateTimeStyles.None, out var date))
                return null;

            if (!TimeSpan.TryParseExact(term.StartAt, "hh\\:mm", CultureInfo.InvariantCulture, out var time))
                return null;

            return date.Add(time);
        }
    }
}