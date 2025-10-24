using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using LangLang.Controller.Services;
using LangLang.Controller.Services.Reports;
using LangLang.Controller.TeachingActivities;
using LangLang.Controller.Users;
using LangLang.Model.Enums;

namespace LangLang.Controller.Messages
{
    internal class LanguageStatisticsReportService
    {
        private readonly CourseController courseController;
        private readonly ExamTermsController examController;
        private readonly PenaltyPointsController penaltyPointsController;
        private readonly StudentsController studentsController;
        private readonly ExamResultsController examResultsController;

        public LanguageStatisticsReportService(
            CourseController courseController,
            ExamTermsController examController,
            StudentsController studentsController,
            ExamResultsController examResultsController)
        {
            this.courseController = courseController;
            this.examController = examController;
            this.studentsController = studentsController;
            this.examResultsController = examResultsController;
            this.penaltyPointsController = new PenaltyPointsController();
        }

        public void GenerateReport()
        {
            var stats = CalculateLanguageStatistics();
            byte[] pdf = CreatePdfDocument(stats);
            string fileName = $"LanguageStatisticsReport({DateTime.Now:yyyy-MM-dd}).pdf";
            Task.Run(() => ReportEmailService.SendReportEmail(pdf, fileName));
        }

        private Dictionary<Language, LanguageStats> CalculateLanguageStatistics()
        {
            DateTime today = DateTime.Now, oneYearAgo = today.AddYears(-1);
            var stats = Enum.GetValues(typeof(Language))
                            .Cast<Language>()
                            .ToDictionary(l => l, _ => new LanguageStats());

            foreach (var course in courseController.GetAll())
            {
                if (!IsActivityInLastYear(course.Date, course.DurationWeeks, oneYearAgo, today))
                    continue;
                if (!Enum.TryParse<Language>(course.Language.ToString(), out Language lang))
                    continue;
                stats[lang].CoursesCount++;
                stats[lang].TotalPenalties += penaltyPointsController.GetByCourseId(course.Id).Count;
            }

            foreach (var exam in examController.GetAll())
            {
                if (!IsExamInLastYear(exam.Date, oneYearAgo, today))
                    continue;
                if (!Enum.TryParse<Language>(exam.Language.ToString(), out Language lang))
                    continue;
                stats[lang].ExamsCount++;
                foreach (var result in examResultsController.GetAll().Where(r => r.ExamId == exam.Id))
                {
                    stats[lang].TotalPoints += result.TotalPoints;
                    stats[lang].PointsCount++;
                }
            }

            return stats;
        }

        private bool IsActivityInLastYear(string date, int weeks, DateTime start, DateTime end)
            => TryParseDate(date, out DateTime d) && d.AddDays(weeks * 7) >= start && d.AddDays(weeks * 7) <= end;

        private bool IsExamInLastYear(string date, DateTime start, DateTime end)
            => TryParseDate(date, out DateTime d) && d >= start && d <= end;

        private bool TryParseDate(string dateString, out DateTime date)
            => DateTime.TryParseExact(dateString, "dd.MM.yyyy", new CultureInfo("sr-RS"), DateTimeStyles.None, out date);

        private byte[] CreatePdfDocument(Dictionary<Language, LanguageStats> stats)
        {
            var lines = new List<string>
            {
                $"Report generated on: {DateTime.Now:yyyy-MM-dd}\n",
                "Language Statistics (last 12 months):\n"
            };

            foreach (var lang in Enum.GetValues(typeof(Language)).Cast<Language>().OrderBy(l => l.ToString()))
            {
                var s = stats[lang];
                lines.Add("═══════════════════════════════════════");
                lines.Add($"LANGUAGE: {lang.ToString().ToUpper()}");
                lines.Add("═══════════════════════════════════════");
                lines.Add($"  Courses created: {s.CoursesCount}");
                lines.Add($"  Exams created: {s.ExamsCount}");
                lines.Add($"  Average penalties per course: {(s.CoursesCount > 0 ? (double)s.TotalPenalties / s.CoursesCount : 0):F2}");
                lines.Add($"  Average points: {(s.PointsCount > 0 ? (double)s.TotalPoints / s.PointsCount : 0):F2}\n");
            }

            return PDFService.GenerateCenteredReport("Language Statistics Report", lines.ToArray());
        }

        private class LanguageStats
        {
            public int CoursesCount { get; set; }
            public int ExamsCount { get; set; }
            public int TotalPenalties { get; set; }
            public int TotalPoints { get; set; }
            public int PointsCount { get; set; }
        }
    }
}
