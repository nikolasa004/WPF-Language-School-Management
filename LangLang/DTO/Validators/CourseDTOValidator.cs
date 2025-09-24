using System;
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

        public string Validate(CourseDTO course, string propertyName) => propertyName switch
        {
            nameof(CourseDTO.Date) => string.IsNullOrWhiteSpace(course.Date) || !DateTime.TryParseExact(course.Date, "dd.MM.yyyy", Sr, DateTimeStyles.None, out _) ? "Invalid course start date (dd.MM.yyyy)." : string.Empty,
            nameof(CourseDTO.StartAt) =>string.IsNullOrWhiteSpace(course.StartAt) || !TimeSpan.TryParseExact(course.StartAt, "hh\\:mm", CultureInfo.InvariantCulture, out _)? "Invalid start time (HH:mm)." : string.Empty,
            nameof(CourseDTO.DurationWeeks) =>course.DurationWeeks <= 0 ? "DurationWeeks must be greater than 0." : string.Empty,
            nameof(CourseDTO.Days) =>(course.Days == null || course.Days.Count == 0) ? "Course must have at least one day of week." : string.Empty,
            nameof(CourseDTO.MaxAttendees) =>(course.IsLive && (!course.MaxAttendees.HasValue || course.MaxAttendees.Value <= 0))? "For live courses, MaxAttendees must be greater than 0." : string.Empty,

            _ => string.Empty
        };

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