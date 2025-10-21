using LangLang.Controller.Services;
using LangLang.Controller.TeachingActivities;
using LangLang.Controller.Users;
using LangLang.DTO;
using LangLang.Model.TeachingActivities;
using LangLang.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace LangLang.Controller.Messages
{
    internal class ReportsController
    {
        private StudentsController studentsController;
        private ExamResultsController examResultsController;
        private PenaltyPointsController penaltyPointsController;
        private CourseController courseController;
        private ProfessorsController professorsController;

        public ReportsController(StudentsController studentsController, ExamResultsController examResultsController, CourseController courseController, ProfessorsController professorsController)
        {
            this.studentsController = studentsController;
            this.examResultsController = examResultsController;
            penaltyPointsController = new PenaltyPointsController();
            this.courseController = courseController;
            this.professorsController = professorsController;
        }

        public void GenerateReport(string reportType)
        {
            if (reportType == "penalty")
            {
                GeneratePenaltyReport();
            }
            if (reportType == "average")
            {
                GenerateAverageGradesReport();
            }
        }

        public void GenerateAverageGradesReport()
        {
            DateTime today = DateTime.Now;
            DateTime oneYearAgo = today.AddYears(-1);

            var allCourses = courseController.GetAll();
            var professors = professorsController.GetAllProfessors();

            Dictionary<int, (double avgKnowledge, double avgActivity)> courseAverages =
                new Dictionary<int, (double, double)>();

            foreach (var course in allCourses)
            {
                if (!DateTime.TryParseExact(course.Date, "dd.MM.yyyy", new CultureInfo("sr-RS"), DateTimeStyles.None, out DateTime startDate))
                    continue;

                DateTime endDate = startDate.AddDays(course.DurationWeeks * 7);
                if (endDate < oneYearAgo || endDate > today)
                    continue; // kurs nije u opsegu poslednjih godinu dana

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
                    double avgKnowledge = sumKnowledge / gradeCount;
                    double avgActivity = sumActivity / gradeCount;
                    courseAverages[course.Id] = (avgKnowledge, avgActivity);
                }
            }

           
            Dictionary<string, double> professorAverages = new Dictionary<string, double>();
            foreach (var professor in professors)
            {
                int sum = professor.AverageRating.Item1;
                int count = professor.AverageRating.Item2;

                double avg = count > 0 ? (double)sum / count : 0;
                professorAverages[professor.Email] = avg;
            }

          
            MakeAverageGradesPdf(courseAverages, professorAverages);
        }

        void MakeAverageGradesPdf(
            Dictionary<int, (double avgKnowledge, double avgActivity)> courseAverages,
            Dictionary<string, double> professorAverages)
        {
            string title = "Average Grades Report";
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd");

            List<string> lines = new List<string>
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

            byte[] document = PDFService.GenerateCenteredReport(title, lines.ToArray());
            Task.Run(() => SendReportEmail(document, $"AverageGradesReport({currentDate}).pdf"));
        }



        private void GeneratePenaltyReport()
        {
            List<int> pointsSum = new List<int> { 0, 0, 0, 0 };
            List<int> studentCount = new List<int> { 0, 0, 0, 0 };

            foreach (ExamResult examResult in examResultsController.GetAll())
            {
                var student = studentsController.GetStudentById(examResult.StudentId);
                if (student != null)
                {
                    pointsSum[student.PenaltyPoints] += examResult.TotalPoints;
                    studentCount[student.PenaltyPoints]++;
                }
            }

            Dictionary<int, int> coursesPenalties = new Dictionary<int, int>();
            foreach (var course in courseController.GetAll())
            {
                coursesPenalties[course.Id] = penaltyPointsController.GetByCourseId(course.Id).Count;
            }

            MakePenaltyPdf(pointsSum, studentCount, coursesPenalties);
        }
        private void MakePenaltyPdf(List<int> pointsSum, List<int> studentCount, Dictionary<int,int> coursesPenalties)
        {
            string title = "Penalty Points Report";
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd");

            string[] courseLines = MakeCoursePenaltyLines(coursesPenalties);
            string[] averagePointLines = MakeAveragePenaltyLines(pointsSum, studentCount);

            var lines = new[] { $"Report generated on: {currentDate}\n" }.Concat(courseLines ?? Array.Empty<string>()).Concat(Array.Empty<string>()).Concat(averagePointLines ?? Array.Empty<string>()).ToArray();

            byte[] document=PDFService.GenerateCenteredReport(title, lines);
            Task.Run(() => SendReportEmail(document,"PenaltyReport("+currentDate+").pdf"));
        }
        private string[] MakeCoursePenaltyLines(Dictionary<int, int> coursesPenalties)
        {
            string[] lines = new string[courseController.GetAll().Count];
            List<Course> allCourses = courseController.GetAll();
            for (int i = 0; i < courseController.GetAll().Count; i++)
            {
                var course = allCourses[i];
                CourseDTO courseDTO = new CourseDTO(course);
                var start = GUIHelper.GetCourseDate(courseDTO);
                var end = start?.AddDays(7 * courseDTO.DurationWeeks);
                if ((start >= DateTime.Now.AddYears(-1) && start <= DateTime.Now) || (end >= DateTime.Now.AddYears(-1) && end <= DateTime.Now))
                {
                    lines[i] = "Course-" + course.Id + " had " + coursesPenalties[course.Id] + " penalty points assigned.";
                }
            }
            return lines;
        }
        private string[] MakeAveragePenaltyLines(List<int> pointsSum, List<int> studentCount)
        {
            string[] lines = new string[4];
            for (int i = 0; i < pointsSum.Count; i++)
            {
                if (studentCount[i] > 0)
                {
                    lines[i] = "Average points for students with " + i + " penalty points: " + (pointsSum[i] / studentCount[i]);
                }
                else
                {
                    lines[i] = "Average points for students with " + i + " penalty points: N/A";
                }
            }
            return lines;
        }

        static async Task SendReportEmail(byte[] pdfBytes,string reportName)
        {
            await EmailService.SendReportAsync(
            smtpHost: "localhost",
            smtpPort: 1025,
            fromName: "Language App",
            fromEmail: "no-reply@local.test",
            toName: "Principal",
            toEmail: "principal@gmail.com",
            subject: "Generated report",
            bodyText: "Hi! Here is your generated report!",
            pdfBytes: pdfBytes,
            pdfFileName: reportName
        );
        }
    }
}
