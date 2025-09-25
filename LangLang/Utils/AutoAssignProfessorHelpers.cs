using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LangLang.Model.Enums;
using LangLang.Model.TeachingActivities;
using LangLang.Model.Users;
namespace LangLang.Utils
{
    public static class AutoAssignHelpers
    {
        public static IEnumerable<Professor> FilterQualified(
            IEnumerable<Professor> allProfessors,
            Language language,
            LanguageLevel level,
            bool exactLevel = true)
        {
            return (allProfessors ?? Enumerable.Empty<Professor>())
                .Where(p => p?.Languages != null
                         && p.Languages.TryGetValue(language, out var profLevel)
                         && (exactLevel ? profLevel == level : profLevel >= level));
        }
        public static IEnumerable<Professor> FilterAvailable(IEnumerable<Professor> candidates,IEnumerable<Course> allCourses,IEnumerable<int> days,string startAt,int durationMinutes = 90)
        {
            if (!TimeSpan.TryParseExact(startAt ?? "", new[] { @"hh\:mm", @"h\:mm" },
                                        CultureInfo.InvariantCulture, out var newStart))
                return Enumerable.Empty<Professor>();
            var newEnd = newStart + TimeSpan.FromMinutes(durationMinutes);
            var daySet = new HashSet<DayOfWeek>((days ?? Enumerable.Empty<int>())
                            .Where(d => d >= 1 && d <= 7)
                            .Select(d => d == 7 ? DayOfWeek.Sunday : (DayOfWeek)d));
            var assigned = (allCourses ?? Enumerable.Empty<Course>()).Where(c => !string.IsNullOrWhiteSpace(c?.ProfessorId));
            bool NoCollision(Course ex)
            {
                if (ex?.Days == null || ex.Days.Count == 0) return false; 
                if (!ex.Days.Any(d => daySet.Contains(d))) return true;  

                if (!TimeSpan.TryParseExact(ex.StartAt ?? "", new[] { @"hh\:mm", @"h\:mm" },
                                            CultureInfo.InvariantCulture, out var exStart))
                    return false;
                var exDur = ex.Duration > 0 ? ex.Duration : 90;
                var exEnd = exStart + TimeSpan.FromMinutes(exDur);
                return !(newStart < exEnd && exStart < newEnd); 
            }
            return (candidates ?? Enumerable.Empty<Professor>())
                 .Where(p => assigned.Where(c => c.ProfessorId == p.Email).All(NoCollision));
        }
        public static Professor? PickByRating(IEnumerable<Professor> candidates)
        {
            return (candidates ?? Enumerable.Empty<Professor>())
                .Select(p => new
                {
                    Prof = p,
                    Avg = (p?.AverageRating.Item2 ?? 0) == 0
                          ? 0.0
                          : (double)p!.AverageRating.Item1 / p.AverageRating.Item2,
                    Cnt = p?.AverageRating.Item2 ?? 0
                })
                .OrderByDescending(x => x.Avg)
                .ThenByDescending(x => x.Cnt)
                .Select(x => x.Prof)
                .FirstOrDefault();
        }
        public static Professor? SmartPickProfessor(IEnumerable<Professor> allProfessors,IEnumerable<Course> allCourses,Language language,LanguageLevel level,IEnumerable<int> days,   string startAt,bool exactLevel = true)
        {
            var q = FilterQualified(allProfessors, language, level, exactLevel).ToList();
            if (q.Count == 0) return null;

            var a = FilterAvailable(q, allCourses, days, startAt).ToList();
            if (a.Count == 0) return null;

            if (a.Count == 1) return a[0];
            return PickByRating(a);
        }
    }
}
