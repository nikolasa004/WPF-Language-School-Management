using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LangLang.Model.TeachingActivities;
using LangLang.Observer;
using LangLang.Storage;

namespace LangLang.Model.DAO.TeachingActivity
{
    internal class CoursesDAO
    {
        private readonly List<Course> _courses;
        private readonly Storage<Course> _storage;
        public Subject CourseSubject;
        private readonly SortedSet<int> _freeIds;
        private int _maxId;

        private static readonly CultureInfo Sr = new("sr-RS");
        private const string SrDateFmt = "dd.MM.yyyy";
        private const string IsoDateFmt = "yyyy-MM-dd";

        public CoursesDAO()
        {
            _storage = new Storage<Course>("courses.json");
            _courses = _storage.Load();

            foreach (var c in _courses)
            {
                if (!string.IsNullOrWhiteSpace(c.Date))
                    c.Date = ToSrOrKeep(c.Date);
            }

            CourseSubject = new Subject();
            _freeIds = RecalculateFreeIds(_courses);
            _maxId = _courses.Count == 0 ? 0 : _courses.Max(c => c.Id);
        }

        private SortedSet<int> RecalculateFreeIds(List<Course> courses)
        {
            var free = new SortedSet<int>();
            if (courses.Count == 0) return free;
            int maxId = courses.Max(c => c.Id);
            var used = new HashSet<int>(courses.Select(c => c.Id));
            for (int i = 1; i <= maxId; i++)
                if (!used.Contains(i)) free.Add(i);
            return free;
        }

        private int GenerateId()
        {
            if (_freeIds.Count > 0)
            {
                int id = _freeIds.Min;
                _freeIds.Remove(id);
                return id;
            }
            return ++_maxId;
        }

        public Course AddCourse(Course course)
        {
            if (course.Id == 0)
                course.Id = GenerateId();
            if (!string.IsNullOrWhiteSpace(course.Date))
                course.Date = ToSrOrKeep(course.Date);

            _courses.Add(course);
            SaveAllIso();
            CourseSubject.NotifyObservers();
            return course;
        }

        public Course? UpdateCourse(Course course)
        {
            var old = GetCourseById(course.Id);
            if (old is null) return null;

            old.ProfessorId = course.ProfessorId;
            old.Language = course.Language;
            old.Level = course.Level;
            old.IsLive = course.IsLive;
            old.MaxAttendees = course.MaxAttendees;
            old.Date = ToSrOrKeep(course.Date);
            old.StartAt = course.StartAt;
            old.RoomId = course.RoomId;
            old.DurationWeeks = course.DurationWeeks;
            old.Days = course.Days;
            old.Duration = 90; // kurs uvek 90min
            old.Grades = course.Grades.ToDictionary(kvp => kvp.Key, kvp => new Grade(kvp.Value.Knowledge, kvp.Value.Activity));

            SaveAllIso();
            CourseSubject.NotifyObservers();
            return old;
        }

        public Course? RemoveCourse(int id)
        {
            var course = GetCourseById(id);
            if (course is null) return null;

            _courses.Remove(course);
            _freeIds.Add(id);
            SaveAllIso();
            CourseSubject.NotifyObservers();
            return course;
        }

        public Course? GetCourseById(int id) => _courses.Find(c => c.Id == id);

        public List<Course> GetAllCourses() => _courses;

        public List<Course> GetCoursesByProfessor(string professorId) => _courses.FindAll(c => c.ProfessorId == professorId);

        private void SaveAllIso()
        {
            var isoCopy = _courses.Select(CloneWithIsoDate).ToList();
            _storage.Save(isoCopy);
        }
        private static Course CloneWithIsoDate(Course src)
        {
            return new Course
            {
                Id = src.Id,
                ProfessorId = src.ProfessorId,
                Language = src.Language,
                Level = src.Level,
                IsLive = src.IsLive,
                MaxAttendees = src.MaxAttendees,
                Date = ToIsoOrKeep(src.Date),
                StartAt = src.StartAt,
                RoomId = src.RoomId,
                Duration = 90,
                DurationWeeks = src.DurationWeeks,
                Days = new List<DayOfWeek>(src.Days),
                Grades = src.Grades.ToDictionary(k => k.Key, v => new Grade(v.Value.Knowledge, v.Value.Activity))
            };
        }
        private static string ToIsoOrKeep(string? date)
        {
            if (string.IsNullOrWhiteSpace(date)) return date ?? string.Empty;
            if (DateTime.TryParseExact(date, IsoDateFmt, CultureInfo.InvariantCulture, DateTimeStyles.None, out var isoParsed))
                return isoParsed.ToString(IsoDateFmt, CultureInfo.InvariantCulture);
            if (DateTime.TryParseExact(date, SrDateFmt, Sr, DateTimeStyles.None, out var srParsed))
                return srParsed.ToString(IsoDateFmt, CultureInfo.InvariantCulture);
            if (DateTime.TryParse(date, out var any))
                return any.ToString(IsoDateFmt, CultureInfo.InvariantCulture);
            return date;
        }
        private static string ToSrOrKeep(string? date)
        {
            if (string.IsNullOrWhiteSpace(date)) return date ?? string.Empty;
            if (DateTime.TryParseExact(date, IsoDateFmt, CultureInfo.InvariantCulture, DateTimeStyles.None, out var isoParsed))
                return isoParsed.ToString(SrDateFmt, Sr);
            if (DateTime.TryParseExact(date, SrDateFmt, Sr, DateTimeStyles.None, out var srParsed))
                return srParsed.ToString(SrDateFmt, Sr);
            if (DateTime.TryParse(date, out var any))
                return any.ToString(SrDateFmt, Sr);
            return date;
        }
    }
}