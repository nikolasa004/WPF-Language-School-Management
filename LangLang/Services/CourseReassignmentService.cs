using System;
using System.Collections.Generic;
using System.Linq;
using LangLang.Controller;
using LangLang.Model;

namespace LangLang.Services
{
    public class CourseReassignmentService
    {
        private readonly CourseController _courseController;
        private readonly ProfessorsController _professorsController;

        public CourseReassignmentService(
            CourseController courseController,
            ProfessorsController professorsController)
        {
            _courseController = courseController;
            _professorsController = professorsController;
        }

        public List<CourseAssignment> BuildAssignments(List<Course> ongoingCourses, Professor professorToDelete)
        {
            var assignments = new List<CourseAssignment>();
            var allProfessors = _professorsController.GetAllProfessors()
                .Where(p => p.Email != professorToDelete.Email)
                .ToList();

            foreach (var course in ongoingCourses)
            {
                var assignment = new CourseAssignment(
                    course,
                    string.Join(", ", course.Days?.Select(d => d.ToString().Substring(0, 3)) ?? new List<string>()),
                    GetCompatibleProfessors(course, allProfessors)
                );

                assignments.Add(assignment);
            }

            return assignments;
        }

        private List<ProfessorOption> GetCompatibleProfessors(Course course, List<Professor> allProfessors)
        {
            var result = new List<ProfessorOption>
            {
                new ProfessorOption("", "-- Select Professor --")
            };

            foreach (var professor in allProfessors)
            {
                if (professor.Languages != null &&
                    professor.Languages.ContainsKey(course.Language) &&
                    professor.Languages[course.Language] >= course.Level &&
                    !HasScheduleConflict(professor, course))
                {
                    result.Add(new ProfessorOption(
                        professor.Email,
                        $"{professor.FirstName} {professor.LastName} ({professor.Email})"
                    ));
                }
            }

            return result;
        }

        private bool HasScheduleConflict(Professor professor, Course newCourse)
        {
            var existingCourses = _courseController.GetMyCourses(professor.Email);
            var newSessions = ExpandToSessions(newCourse);

            foreach (var existing in existingCourses)
            {
                foreach (var s1 in ExpandToSessions(existing))
                    foreach (var s2 in newSessions)
                    {
                        if (s1.start < s2.end && s2.start < s1.end)
                            return true;
                    }
            }

            return false;
        }

        private static List<(DateTime start, DateTime end)> ExpandToSessions(Course c)
        {
            var sessions = new List<(DateTime, DateTime)>();

            if (!DateTime.TryParseExact(c.Date ?? "", "dd.MM.yyyy",
                System.Globalization.CultureInfo.GetCultureInfo("sr-RS"),
                System.Globalization.DateTimeStyles.None, out var baseDate))
                return sessions;

            if (!TimeSpan.TryParseExact(c.StartAt ?? "", "hh\\:mm",
                System.Globalization.CultureInfo.InvariantCulture, out var ts))
                return sessions;

            for (int week = 0; week < c.DurationWeeks; week++)
            {
                foreach (var day in c.Days ?? new List<DayOfWeek>())
                {
                    var sessionDate = baseDate.AddDays(((int)day - (int)baseDate.DayOfWeek + 7) % 7 + week * 7);
                    var start = sessionDate.Date.Add(ts);
                    var end = start.AddMinutes(c.Duration);
                    sessions.Add((start, end));
                }
            }

            return sessions;
        }

        public List<Course> ValidateAssignments(IEnumerable<CourseAssignment> assignments)
        {
            return assignments
                .Where(a => a.SelectedProfessor == null || string.IsNullOrEmpty(a.SelectedProfessor.Email))
                .Select(a => a.Course)
                .ToList();
        }

        public List<string> PerformReassignment(IEnumerable<CourseAssignment> assignments)
        {
            var errors = new List<string>();

            foreach (var assignment in assignments)
            {
                try
                {
                    var course = assignment.Course; 
                    if (assignment.SelectedProfessor == null)
                    {
                        throw new Exception();
                    }
                    course.ProfessorId = assignment.SelectedProfessor.Email;
                    var updated = _courseController.ForceUpdate(course);

                    if (updated == null)
                        errors.Add($"Failed to update Course ID {course.Id}");
                }
                catch (Exception ex)
                {
                    errors.Add($"Course ID {assignment.Course.Id}: {ex.Message}");
                }
            }

            return errors;
        }
    }

}
