using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LangLang.Model;
using LangLang.Model.DAO;

namespace LangLang.Controller
{
    public class CourseController
    {
        private readonly CoursesDAO _dao;
        private static readonly CultureInfo Sr = new("sr-RS");

        public CourseController()
        {
            _dao = new CoursesDAO();
        }

        public List<Course> GetMyCourses(string professorId)
            => _dao.GetCoursesByProfessor(professorId);

        public List<Course> SearchMyCourses(
            string professorId,
            LangLang.Model.Enums.Language? language,
            LangLang.Model.Enums.LanguageLevel? level,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var items = _dao.GetCoursesByProfessor(professorId).AsEnumerable();

            if (language is not null)
                items = items.Where(c => c.Language == language);

            if (level is not null)
                items = items.Where(c => c.Level == level);

            if (fromDate.HasValue && toDate.HasValue)
            {
                var from = fromDate.Value.Date;
                var to = toDate.Value.Date;
                if (from > to) (from, to) = (to, from);

                items = items.Where(c =>
                {
                    var start = TryGetFirstStartDate(c);
                    if (!start.HasValue) return false;
                    return start.Value.Date >= from && start.Value.Date <= to;
                });
            }

            return items.OrderBy(c => TryGetFirstStartDate(c)).ToList();
        }

        public List<Course> SearchCourses(List<Course> input, Model.Enums.Language? language, Model.Enums.LanguageLevel? level, bool? isLive, int weeks, DateTime? fromDate, DateTime? toDate)
        {
            var items = input.AsEnumerable();
            if (language is not null) items = items.Where(t => t.Language == language);
            if (level is not null) items = items.Where(t => t.Level == level);
            if (fromDate.HasValue ^ toDate.HasValue)
            {
                var exact = (fromDate ?? toDate)!.Value.Date;
                fromDate = exact;
                toDate = exact;
            }
            if (fromDate.HasValue && toDate.HasValue)
            {
                var from = fromDate.Value.Date;
                var to = toDate.Value.Date;
                if (from > to) (from, to) = (to, from);
                items = items.Where(t =>
                {
                    var start = TryGetFirstStartDate(t);
                    if (!start.HasValue) return false;
                    var d = start.Value.Date;
                    return d >= from && d <= to;
                });
            }
            if (isLive!= null)
                items = items.Where(t => t.IsLive == isLive);
            if (weeks > 0)
                items = items.Where(t => t.DurationWeeks == weeks);

            return items.OrderBy(t => TryGetFirstStartDate(t)).ToList();
        }

        public Course Create(Course course)
        {
            course.IsLive = true;
            course.Duration = 90;

            EnsureValidBasics(course);
            EnsureNoOverlap(course, ignoreId: null);

            return _dao.AddCourse(course);
        }

        public Course? Update(Course course)
        {
            EnsureCanModify(course.Id);
            course.IsLive = true;
            course.Duration = 90;

            EnsureValidBasics(course);
            EnsureNoOverlap(course, ignoreId: course.Id);

            return _dao.UpdateCourse(course);
        }

        public Course? ForceUpdate(Course course)
        {
            course.IsLive = true;
            course.Duration = 90;
            EnsureNoOverlap(course, ignoreId: course.Id);
            return _dao.UpdateCourse(course);
        }

        public Course? Delete(int id)
        {
            EnsureCanModify(id);
            return _dao.RemoveCourse(id);
        }

        public Course? ForceDelete(int id)
        {
            return _dao.RemoveCourse(id);
        }

        public Course? GetById(int id) => _dao.GetCourseById(id);
        public List<Course> GetAll() => _dao.GetAllCourses();

        private static void EnsureValidBasics(Course c)
        {
            if (c.IsLive && (c.MaxAttendees is null || c.MaxAttendees <= 0))
                throw new ArgumentException("MaxAttendees must be greater than 0.");

            if (string.IsNullOrWhiteSpace(c.Date) ||
                !DateTime.TryParseExact(c.Date, "dd.MM.yyyy", Sr, DateTimeStyles.None, out _))
                throw new ArgumentException("Invalid course start date (dd.MM.yyyy).");

            if (string.IsNullOrWhiteSpace(c.StartAt) ||
                !TimeSpan.TryParseExact(c.StartAt, "hh\\:mm", CultureInfo.InvariantCulture, out _))
                throw new ArgumentException("Invalid start time (HH:mm).");

            if (c.DurationWeeks <= 0)
                throw new ArgumentException("DurationWeeks must be greater than 0.");

            if (c.Days == null || c.Days.Count == 0)
                throw new ArgumentException("Course must have at least one day of week.");
        }

        private void EnsureCanModify(int courseId)
        {
            var current = _dao.GetCourseById(courseId)
                          ?? throw new InvalidOperationException("Course not found.");

            var start = TryGetFirstStartDate(current)
                        ?? throw new InvalidOperationException("Course has invalid date/time.");

            if ((start - DateTime.Now).TotalDays < 7)
                throw new InvalidOperationException("Cannot modify or cancel a course within 7 days of its start.");
        }

        private void EnsureNoOverlap(Course candidate, int? ignoreId)
        {
            var mine = _dao.GetCoursesByProfessor(candidate.ProfessorId);
            var candidateSessions = ExpandToSessions(candidate);

            foreach (var other in mine)
            {
                if (ignoreId.HasValue && other.Id == ignoreId.Value) continue;
                var otherSessions = ExpandToSessions(other);

                foreach (var s in candidateSessions)
                {
                    foreach (var o in otherSessions)
                    {
                        bool overlap = s.start < o.end && o.start < s.end;
                        if (overlap)
                            throw new InvalidOperationException("Professor already has a course overlapping in that time window.");
                    }
                }
            }
        }

        /// <summary>
        /// Pretvori kurs u listu termina (svaka nedelja, svaki dan).
        /// </summary>
        private static List<(DateTime start, DateTime end)> ExpandToSessions(Course c)
        {
            var sessions = new List<(DateTime, DateTime)>();

            if (!DateTime.TryParseExact(c.Date ?? "", "dd.MM.yyyy", Sr, DateTimeStyles.None, out var baseDate))
                return sessions;

            if (!TimeSpan.TryParseExact(c.StartAt ?? "", "hh\\:mm", CultureInfo.InvariantCulture, out var ts))
                return sessions;

            for (int week = 0; week < c.DurationWeeks; week++)
            {
                foreach (var day in c.Days)
                {
                    var sessionDate = GetNextWeekday(baseDate.AddDays(7 * week), day);
                    var start = sessionDate.Date.Add(ts);
                    var end = start.AddMinutes(c.Duration);
                    sessions.Add((start, end));
                }
            }
            return sessions;
        }

        private static DateTime GetNextWeekday(DateTime start, DayOfWeek targetDay)
        {
            int daysToAdd = ((int)targetDay - (int)start.DayOfWeek + 7) % 7;
            return start.AddDays(daysToAdd);
        }

        private static DateTime? TryGetFirstStartDate(Course c)
        {
            var sessions = ExpandToSessions(c);
            return sessions.Count > 0 ? sessions.Min(s => s.start) : null;
        }
    }
}

