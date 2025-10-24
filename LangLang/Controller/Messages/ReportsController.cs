using System;
using System.Collections.Generic;
using System.Linq;
using LangLang.Controller.Services.Reports;
using LangLang.Controller.TeachingActivities;
using LangLang.Controller.Users;

namespace LangLang.Controller.Messages
{
    internal class ReportsController
    {
        private readonly Dictionary<string, IReportService> _reportServices;

        public ReportsController(
            StudentsController studentsController,
            ExamResultsController examResultsController,
            CourseController courseController,
            ExamTermsController examTermsController,
            ProfessorsController professorsController)
        {
            var services = new List<IReportService>
            {
                new PenaltyReportService(studentsController, examResultsController, courseController),
                new AverageGradesReportService(courseController, professorsController),
                new LanguageStatisticsReportService(courseController, examTermsController, studentsController, examResultsController),
                new ExamResultsReportService(examResultsController, examTermsController)
            };

            _reportServices = services.ToDictionary(
                reportService => reportService.Key, 
                reportService => reportService,
                StringComparer.OrdinalIgnoreCase
            );
        }

        public void GenerateReport(string reportType)
        {
            if (_reportServices.TryGetValue(reportType, out var service))
            {
                service.GenerateReport();
            }
            else
            {
                throw new ArgumentException($"Unknown report type: {reportType}");
            }
        }
    }
}
