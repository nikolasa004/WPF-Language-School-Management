using LangLang.DTO;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace LangLang.DTO.Validators
{
    class CourseDTOValidator
    {
        private static readonly CultureInfo Sr = new("sr-RS");

        private readonly string[] _validatedProperties =
        {
            nameof(CourseDTO.Language),
            nameof(CourseDTO.Level),
            nameof(CourseDTO.Date),
            nameof(CourseDTO.StartAt),
            nameof(CourseDTO.DurationWeeks),
            nameof(CourseDTO.Days),
            nameof(CourseDTO.MaxAttendees)
        };

        public string Validate(CourseDTO course, string propertyName)
        {
            switch (propertyName)
            {
                case nameof(CourseDTO.Language):
               
                    break;

                case nameof(CourseDTO.Level):
                
                    break;

                case nameof(CourseDTO.Date):
                    if (string.IsNullOrWhiteSpace(course.Date) ||
                        !DateTime.TryParseExact(course.Date, "dd.MM.yyyy", Sr, DateTimeStyles.None, out _))
                        return "Invalid course start date (dd.MM.yyyy).";
                    break;

                case nameof(CourseDTO.StartAt):
                    if (string.IsNullOrWhiteSpace(course.StartAt) ||
                        !TimeSpan.TryParseExact(course.StartAt, "hh\\:mm", CultureInfo.InvariantCulture, out _))
                        return "Invalid start time (HH:mm).";
                    break;

                case nameof(CourseDTO.DurationWeeks):
                    if (course.DurationWeeks <= 0)
                        return "DurationWeeks must be greater than 0.";
                    break;

                case nameof(CourseDTO.Days):
                    if (course.Days == null || course.Days.Count == 0)
                        return "Course must have at least one day of week.";
                    break;

                case nameof(CourseDTO.MaxAttendees):
                    if (course.IsLive && (!course.MaxAttendees.HasValue || course.MaxAttendees.Value <= 0))
                        return "For live courses, MaxAttendees must be greater than 0.";
                    break;
            }

            return string.Empty;
        }

        public bool IsValid(CourseDTO course)
        {
            foreach (var prop in _validatedProperties)
            {
                if (Validate(course, prop) != string.Empty)
                    return false;
            }
            return true;
        }
    }
}
