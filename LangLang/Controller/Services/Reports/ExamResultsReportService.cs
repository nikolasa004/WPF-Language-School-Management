using LangLang.Controller.TeachingActivities;
using LangLang.Model.TeachingActivities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
namespace LangLang.Controller.Services.Reports
{
    internal class ExamResultsReportService : IReportService
    {
        public string Key => "exam-results";
            private readonly ExamResultsController examResultsController;
        private readonly ExamTermsController examTermsController;
        public ExamResultsReportService(
            ExamResultsController examResultsController,
            ExamTermsController examTermsController)
        {
            this.examResultsController = examResultsController;
            this.examTermsController = examTermsController;
        }
        public void GenerateReport() {
            DateTime today = DateTime.Now;
            DateTime oneYearAgo = today.AddYears(-1);
            var results = examResultsController.GetAll();
            var filtered = new List<(ExamResult ER, ExamTerm Term)>();
            foreach (var result in results)
            {
                var term = examTermsController.GetById(result.ExamId);
                if (term == null) continue;
                DateTime? examDate = ParseExamDate(term.Date);
                if (examDate.HasValue && examDate.Value >= oneYearAgo && examDate.Value <= today)
                {
                    filtered.Add((result, term));
                }
            }
            var averages = CalculateAverages(filtered.Select(x => x.ER));
            var stats = CalculateStats(filtered.Select(x => new
            {
                x.ER,
                Key = $"{x.Term.Language}_{x.Term.Level}",
                Label = $"{x.Term.Language} {x.Term.Level}"
            }));
            var pdf = CreatePdf(averages, stats, oneYearAgo, today);
            var fileName = $"ExamResultsReport({today:yyyy-MM-dd}).pdf";
            Task.Run(() => ReportEmailService.SendReportEmail(pdf, fileName));
        }
        private static DateTime? ParseExamDate(string? date) {
            if (string.IsNullOrWhiteSpace(date)) return null;

            string[] formats = { "yyyy-MM-dd", "dd.MM.yyyy", "MM/dd/yyyy" };

            foreach (var f in formats)
            {
                if (DateTime.TryParseExact(date, f, CultureInfo.InvariantCulture,
                                           DateTimeStyles.None, out var parsed))
                    return parsed;
            }
            if (DateTime.TryParse(date, out var any)) return any;
            return null;
        }
        private static (double Reading, double Writing, double Listening, double Speaking)
            CalculateAverages(IEnumerable<ExamResult> results) {
            int n = 0;
            long r = 0, w = 0, l = 0, s = 0;
            foreach (var er in results)
            {
                r += er.ReadingPoints;
                w += er.WritingPoints;
                l += er.ListeningPoints;
                s += er.SpeakingPoints;
                n++;
            }
            if (n == 0) return (0, 0, 0, 0);
            return (
                Math.Round(r * 1.0 / n, 2),
                Math.Round(w * 1.0 / n, 2),
                Math.Round(l * 1.0 / n, 2),
                Math.Round(s * 1.0 / n, 2)
            ); }
        private static Dictionary<string, (string Label, int Attended, int Passed, double Percent)>
            CalculateStats(IEnumerable<dynamic> lastYear) {
            var result = new Dictionary<string, (string, int, int, double)>(StringComparer.OrdinalIgnoreCase);
            foreach (var g in lastYear.GroupBy(x => (string)x.Key!))
            {
                string label = g.First().Label;
                var students = g
                    .Select(x => (string)x.ER.StudentId)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                int attended = students.Count();
                var passedSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var x in g)
                {
                    var sid = (string)x.ER.StudentId;
                    if (x.ER.Passed && !string.IsNullOrWhiteSpace(sid))
                        passedSet.Add(sid);
                }
                int passed = passedSet.Count;
                double pct = attended > 0 ? Math.Round(passed * 100.0 / attended, 2) : 0.0;
                result[g.Key] = (label, attended, passed, pct);
            }
            return result; }
        private static byte[] CreatePdf(
           (double Reading, double Writing, double Listening, double Speaking) avg,
            Dictionary<string, (string Label, int Attended, int Passed, double Percent)> stats,
            DateTime from, DateTime to)
        {
            var lines = new List<string>
                {
                    $"Period: {from:yyyy-MM-dd} — {to:yyyy-MM-dd}",
                    "",
                    "Average points per exam part (last 12 months):",
                    $"  • Reading (max 60):   {avg.Reading:F2}",
                    $"  • Writing (max 60):   {avg.Writing:F2}",
                    $"  • Listening (max 40): {avg.Listening:F2}",
                    $"  • Speaking (max 50):  {avg.Speaking:F2}",
                    "",
                    "Course statistics (attended / passed / % passed):"
                };
            if (stats.Count == 0) {
                lines.Add("  (no data in selected period)");
            }
            else {
                foreach (var kv in stats.OrderBy(k => k.Value.Label, StringComparer.OrdinalIgnoreCase))
                {
                    var (label, att, pass, pct) = kv.Value;
                    lines.Add($"  • {label}: attended={att}, passed={pass}, passed%={pct:F2}");
                }
            }
            return PDFService.GenerateCenteredReport("Exam Results Report", lines.ToArray()); }
        private static DateTime? GetExamTermDate(ExamTerm? term)
        {
            if (term == null || string.IsNullOrWhiteSpace(term.Date)) return null;
            if (DateTime.TryParseExact(term.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                       DateTimeStyles.None, out var d1)) return d1;
            if (DateTime.TryParse(term.Date, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d2)) return d2;
            return null; }
        private static string? GetGroupKey(ExamTerm? term)
            => term == null ? null : $"{term.Language}_{term.Level}";
        private static string BuildGroupLabel(ExamTerm? term)
        {
            if (term == null) return "Unknown";
            return $"{term.Language} {term.Level} (Term {term.Id})";
        }
    } 
}