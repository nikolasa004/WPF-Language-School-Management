using LangLang.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace LangLang.Utils
{
    public class GUIHelper
    {
        public static void ChangePanel(List<Grid> panels, int activateIndex)
        {
            for (int i = 0; i < panels.Count; i++)
            {
                if (i == activateIndex)
                    panels[i].Visibility = System.Windows.Visibility.Visible;
                else
                    panels[i].Visibility = System.Windows.Visibility.Collapsed;
            }
        }
        public static DateTime? GetCourseDate(CourseDTO course)
        {
            if (!DateTime.TryParseExact(course.Date ?? "", "dd.MM.yyyy", new CultureInfo("sr-RS"), DateTimeStyles.None, out var date) && !DateTime.TryParseExact(course.Date ?? "", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                return null;

            if (!TimeSpan.TryParseExact(course.StartAt ?? "", @"hh\:mm", CultureInfo.InvariantCulture, out var timeStamp))
                return null;

            return date.Date.Add(timeStamp);
        }
        public static IEnumerable<CourseDTO> SortCourses(IEnumerable<CourseDTO> source, string sortKey, ListSortDirection direction)
        {
            var key = NormalizeCourseKey(sortKey);

            Func<CourseDTO, object> keySelector = key switch
            {
                "Id" => course => course.Id,
                "Language" => course => course.Language.ToString(),
                "Level" => course => (int)course.Level,
                "ProfessorId" => course => course.ProfessorId,
                "DurationWeeks" => course => course.DurationWeeks,
                "FormattedDays" => course => course.DaysFormatted,
                "IsLive" => course => course.IsLive,
                "RoomId" => course => course.RoomId ?? "",
                "MaxAttendees" => course => course.MaxAttendees ?? 0,
                "StartAt" => course => course.StartAt,
                "Date" => course => GetCourseDate(course) ?? DateTime.MinValue,
                _ => course => GetCourseDate(course) ?? DateTime.MinValue
            };

            return direction == ListSortDirection.Ascending ? source.OrderBy(keySelector) : source.OrderByDescending(keySelector);
        }
        public static string NormalizeCourseKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return "Date";
            key = key.Trim();

            return key.ToLower() switch
            {
                "id" => "Id",
                "language" => "Language",
                "level" => "Level",
                "professor" => "ProfessorId",
                "weeks" => "DurationWeeks",
                "days" => "FormattedDays",
                "online" => "IsLive",
                "start date" => "Date",
                "start" => "StartAt",
                "room" => "RoomId",
                "max" => "MaxAttendees",
                _ => key
            };
        }
    }
}