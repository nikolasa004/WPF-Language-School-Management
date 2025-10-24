using LangLang.Controller.TeachingActivities;
using LangLang.Controller.Users;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace LangLang.Controller.Services.Reports
{
    internal class AverageGradesReportService : IReportService
    {
        public string Key => "average";
        private readonly CourseController courseController;
        private readonly ProfessorsController professorsController;

        public AverageGradesReportService(
            CourseController courseController,
            ProfessorsController professorsController)
        {
            this.courseController = courseController;
            this.professorsController = professorsController;
        }

        public void GenerateReport()
        {
            var courseAverages = CalculateCourseAverages();
            var professorAverages = CalculateProfessorAverages();

            byte[] pdfDocument = CreatePdfDocument(courseAverages, professorAverages);
            string fileName = $"AverageGradesReport({DateTime.Now:yyyy-MM-dd}).pdf";

            Task.Run(() => ReportEmailService.SendReportEmail(pdfDocument, fileName));
        }

        private Dictionary<int, (double avgKnowledge, double avgActivity)> CalculateCourseAverages()
        {
            DateTime today = DateTime.Now;
            DateTime oneYearAgo = today.AddYears(-1);
            var allCourses = courseController.GetAll();
            var courseAverages = new Dictionary<int, (double, double)>();

            foreach (var course in allCourses)
            {
                if (!TryParseCourseDate(course.Date, out DateTime startDate))
                    continue;

                DateTime endDate = startDate.AddDays(course.DurationWeeks * 7);

                if (!IsCourseInLastYear(startDate, endDate, oneYearAgo, today))
                    continue;

                var (avgKnowledge, avgActivity, hasGrades) = CalculateGradeAverages(course);

                if (hasGrades)
                {
                    courseAverages[course.Id] = (avgKnowledge, avgActivity);
                }
            }

            return courseAverages;
        }

        private bool TryParseCourseDate(string dateString, out DateTime date)
        {
            return DateTime.TryParseExact(
                dateString,
                "dd.MM.yyyy",
                new CultureInfo("sr-RS"),
                DateTimeStyles.None,
                out date
            );
        }

        private bool IsCourseInLastYear(DateTime startDate, DateTime endDate, DateTime oneYearAgo, DateTime today)
        {
            return endDate >= oneYearAgo && endDate <= today;
        }

        private (double avgKnowledge, double avgActivity, bool hasGrades) CalculateGradeAverages(dynamic course)
        {
            double sumKnowledge = 0;
            double sumActivity = 0;
            int gradeCount = 0;

            foreach (var kvp in course.Grades)
            {
                var grade = kvp.Value;
                sumKnowledge += grade.Knowledge;
                sumActivity += grade.Activity;
                gradeCount++;
            }

            if (gradeCount > 0)
            {
                return (sumKnowledge / gradeCount, sumActivity / gradeCount, true);
            }

            return (0, 0, false);
        }

        private Dictionary<string, double> CalculateProfessorAverages()
        {
            var professorAverages = new Dictionary<string, double>();
            var professors = professorsController.GetAllProfessors();

            foreach (var professor in professors)
            {
                int sum = professor.AverageRating.Item1;
                int count = professor.AverageRating.Item2;
                double average = count > 0 ? (double)sum / count : 0;

                professorAverages[professor.Email] = average;
            }

            return professorAverages;
        }

        private byte[] CreatePdfDocument(
            Dictionary<int, (double avgKnowledge, double avgActivity)> courseAverages,
            Dictionary<string, double> professorAverages)
        {
            string title = "Average Grades Report";
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd");

            var lines = new List<string>
            {
                $"Report generated on: {currentDate}\n",
                "Course Average Grades (last 12 months):"
            };

            foreach (var kvp in courseAverages)
            {
                int courseId = kvp.Key;
                var (avgKnowledge, avgActivity) = kvp.Value;
                lines.Add($"- Course {courseId}: Knowledge = {avgKnowledge:F2}, Activity = {avgActivity:F2}");
            }

            lines.Add("\nProfessor Average Ratings:");
            foreach (var kvp in professorAverages)
            {
                lines.Add($"- {kvp.Key}: {kvp.Value:F2}");
            }

            return PDFService.GenerateCenteredReport(title, lines.ToArray());
        }
    }
}