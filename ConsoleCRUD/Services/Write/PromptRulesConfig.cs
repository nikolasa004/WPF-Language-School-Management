using System;
using System.Collections.Generic;
using ConsoleCRUD.Services.Write;
using LangLang.Model.Enums;
using LangLang.Model.Users;
using LangLang.Model.TeachingActivities;
using LangLang.Model.Requests;

namespace ConsoleCRUD
{
    internal static class PromptRulesConfig
    {
        public static void RegisterDefaults(){
            PromptRules.Skip<Course>(nameof(Course.Id));
            PromptRules.SkipWithDefault<Course>(nameof(Course.Grades), new Dictionary<string, Grade>());
            PromptRules.Skip<CourseEnrollment>(nameof(CourseEnrollment.ID));
            PromptRules.Skip<CourseEnrollment>(nameof(CourseEnrollment.Status));
            PromptRules.Skip<ExamEnrollment>(nameof(ExamEnrollment.ID));
            PromptRules.Skip<ExamEnrollment>(nameof(ExamEnrollment.Status));
            PromptRules.Skip<ExamResult>(nameof(ExamResult.Id));
            PromptRules.Skip<ExamTerm>(nameof(ExamTerm.Id));
            PromptRules.SkipWithDefault<ExamTerm>(nameof(ExamTerm.IsLocked), false);
            PromptRules.SkipWithDefault<ExamTerm>(nameof(ExamTerm.IsArchived), false);
            PromptRules.Skip<ExamTerm>(nameof(ExamTerm.Duration));
            PromptRules.Skip<PenaltyPoint>(nameof(PenaltyPoint.Id));
            PromptRules.SkipWithDefault<Professor>(nameof(Professor.AverageRating), Tuple.Create(0, 0));
            PromptRules.SkipWithDefault<Student>(nameof(Student.PenaltyPoints), 0);
            PromptRules.SkipWithDefault<Student>(nameof(Student.LastPenaltyProcessedAt), DateTime.Now.ToString("yyyy-MM-dd"));
            PromptRules.SkipWithDefault<Student>(nameof(Student.RequestedCourses), new List<int>());
            PromptRules.SkipWithDefault<Student>(nameof(Student.ActiveCourseId), -1);
            PromptRules.SkipWithDefault<Student>(nameof(Student.FinishedCourses), new List<Tuple<Language, LanguageLevel>>());
            PromptRules.SkipWithDefault<Student>(nameof(Student.RequestedExamTerms), new List<int>());
            PromptRules.SkipWithDefault<Student>(nameof(Student.ActiveExamTermId), -1);
            PromptRules.SkipWithDefault<Student>(nameof(Student.FinishedExams), new List<Tuple<Language, LanguageLevel>>());
            PromptRules.SkipWithDefault<Student>(nameof(Student.IsExamApplicationLocked), false);
        }
    }
}
