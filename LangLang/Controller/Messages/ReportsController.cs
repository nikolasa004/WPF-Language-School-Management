using System;
using LangLang.Controller.TeachingActivities;
using LangLang.Controller.Users;
using LangLang.Controller.Services.Reports;


namespace LangLang.Controller.Messages
{
    internal class ReportsController
    {
        private readonly PenaltyReportService penaltyReportService;
        private readonly AverageGradesReportService averageGradesReportService;

        public ReportsController(
            StudentsController studentsController,
            ExamResultsController examResultsController,
            CourseController courseController,
            ProfessorsController professorsController)
        {
            penaltyReportService = new PenaltyReportService(
                studentsController,
                examResultsController,
                courseController
            );

            averageGradesReportService = new AverageGradesReportService(
                courseController,
                professorsController
            );
        }

        public void GenerateReport(string reportType)
        {
            switch (reportType.ToLower())
            {
                case "penalty":
                    penaltyReportService.GenerateReport();
                    break;
                case "average":
                    averageGradesReportService.GenerateReport();
                    break;
                default:
                    throw new ArgumentException($"Unknown report type: {reportType}");
            }
        }
    }
}