using LangLang.Controller.TeachingActivities;
using LangLang.Controller.Users;
using LangLang.DTO;
using LangLang.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LangLang.Controller.Services.Reports
{
    internal class PenaltyReportService : IReportService
    {
        public string Key => "penalty";
        private readonly StudentsController studentsController;
        private readonly ExamResultsController examResultsController;
        private readonly PenaltyPointsController penaltyPointsController;
        private readonly CourseController courseController;

        public PenaltyReportService(
            StudentsController studentsController,
            ExamResultsController examResultsController,
            CourseController courseController)
        {
            this.studentsController = studentsController;
            this.examResultsController = examResultsController;
            this.courseController = courseController;
            this.penaltyPointsController = new PenaltyPointsController();
        }

        public void GenerateReport()
        {
            var (pointsSum, studentCount) = CalculateAveragePointsByPenalties();
            var coursesPenalties = CalculateCoursePenalties();

            byte[] pdfDocument = CreatePdfDocument(pointsSum, studentCount, coursesPenalties);
            string fileName = $"PenaltyReport({DateTime.Now:yyyy-MM-dd}).pdf";

            Task.Run(() => ReportEmailService.SendReportEmail(pdfDocument, fileName));
        }

        private (List<int> pointsSum, List<int> studentCount) CalculateAveragePointsByPenalties()
        {
            List<int> pointsSum = new List<int> { 0, 0, 0, 0 };
            List<int> studentCount = new List<int> { 0, 0, 0, 0 };

            foreach (var examResult in examResultsController.GetAll())
            {
                var student = studentsController.GetStudentById(examResult.StudentId);
                if (student != null)
                {
                    pointsSum[student.PenaltyPoints] += examResult.TotalPoints;
                    studentCount[student.PenaltyPoints]++;
                }
            }

            return (pointsSum, studentCount);
        }

        private Dictionary<int, int> CalculateCoursePenalties()
        {
            var coursesPenalties = new Dictionary<int, int>();

            foreach (var course in courseController.GetAll())
            {
                coursesPenalties[course.Id] = penaltyPointsController.GetByCourseId(course.Id).Count;
            }

            return coursesPenalties;
        }

        private byte[] CreatePdfDocument(
            List<int> pointsSum,
            List<int> studentCount,
            Dictionary<int, int> coursesPenalties)
        {
            string title = "Penalty Points Report";
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd");

            string[] courseLines = BuildCoursePenaltyLines(coursesPenalties);
            string[] averageLines = BuildAveragePenaltyLines(pointsSum, studentCount);

            var allLines = new[] { $"Report generated on: {currentDate}\n" }
                .Concat(courseLines ?? Array.Empty<string>())
                .Concat(new[] { "" })
                .Concat(averageLines ?? Array.Empty<string>())
                .ToArray();

            return PDFService.GenerateCenteredReport(title, allLines);
        }

        private string[] BuildCoursePenaltyLines(Dictionary<int, int> coursesPenalties)
        {
            var allCourses = courseController.GetAll();
            var lines = new List<string>();
            DateTime oneYearAgo = DateTime.Now.AddYears(-1);
            DateTime today = DateTime.Now;

            foreach (var course in allCourses)
            {
                var courseDTO = new CourseDTO(course);
                var startDate = GUIHelper.GetCourseDate(courseDTO);
                var endDate = startDate?.AddDays(7 * courseDTO.DurationWeeks);

                if (IsCourseInLastYear(startDate, endDate, oneYearAgo, today))
                {
                    lines.Add($"Course-{course.Id} had {coursesPenalties[course.Id]} penalty points assigned.");
                }
            }

            return lines.ToArray();
        }

        private bool IsCourseInLastYear(DateTime? start, DateTime? end, DateTime oneYearAgo, DateTime today)
        {
            return (start >= oneYearAgo && start <= today) || (end >= oneYearAgo && end <= today);
        }

        private string[] BuildAveragePenaltyLines(List<int> pointsSum, List<int> studentCount)
        {
            string[] lines = new string[4];

            for (int i = 0; i < pointsSum.Count; i++)
            {
                if (studentCount[i] > 0)
                {
                    int average = pointsSum[i] / studentCount[i];
                    lines[i] = $"Average points for students with {i} penalty points: {average}";
                }
                else
                {
                    lines[i] = $"Average points for students with {i} penalty points: N/A";
                }
            }

            return lines;
        }
    }
}