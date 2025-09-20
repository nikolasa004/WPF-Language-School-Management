using System;
using System.Collections.Generic;
using System.Text.Json;
using LangLang.Model.Enums;
using LangLang.Storage.Serialization;

namespace LangLang.Model.TeachingActivities
{
    public class Course : ProfessorTeachingActivity, ISerializable
    {
        public int DurationWeeks { get; set; }

        public List<DayOfWeek> Days { get; set; } = new List<DayOfWeek>();

        public Course() : base()
        {
            Duration = 90;
        }

        public Course(
            int id,
            string professorId,
            Language language,
            LanguageLevel level,
            bool isLive,
            int? maxAttendees,
            string? startDate,
            string? startAt,
            List<DayOfWeek> days,
            int durationWeeks,
            string? roomId = null
        ) : base(id, professorId, language, level, isLive, maxAttendees,
                 date: startDate, startAt: startAt, duration: 90, roomId: roomId)
        {
            Date = startDate;
            StartAt = startAt;
            Days = days;
            DurationWeeks = durationWeeks;

            if (isLive && maxAttendees == null)
            {
                throw new ArgumentException("Course in-person must have MaxAttendees defined.");
            }
        }

        public override string ToString()
        {
            string daysStr = Days.Count > 0 ? string.Join(", ", Days) : "N/A";
            return base.ToString() +
                   $", DurationWeeks: {DurationWeeks}, Days: {daysStr}";
        }

        public string ToJSON()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            return JsonSerializer.Serialize(this, options);
        }

        public void FromJSON(string value)
        {
            Course? course = JsonSerializer.Deserialize<Course>(value);
            if (course != null)
            {
                Id = course.Id;
                ProfessorId = course.ProfessorId;
                Language = course.Language;
                Level = course.Level;
                IsLive = course.IsLive;
                MaxAttendees = course.MaxAttendees;
                Date = course.Date;
                StartAt = course.StartAt;
                Days = course.Days;
                DurationWeeks = course.DurationWeeks;
                Duration = 90;
                RoomId = course.RoomId;
            }
        }
    }
}
