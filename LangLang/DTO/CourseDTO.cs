using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using LangLang.Model;
using LangLang.Model.Enums;

namespace LangLang.DTO
{
    public class CourseDTO : INotifyPropertyChanged, IDataErrorInfo
    {
        public int Id { get; set; }
        public string ProfessorId { get; set; }

        private Language language;
        public Language Language
        {
            get => language;
            set
            {
                if (value != language)
                {
                    language = value;
                    OnPropertyChanged(nameof(Language));
                }
            }
        }

        private LanguageLevel level;
        public LanguageLevel Level
        {
            get => level;
            set
            {
                if (value != level)
                {
                    level = value;
                    OnPropertyChanged(nameof(Level));
                }
            }
        }

        public bool IsLive { get; set; } = true;

        private int? maxAttendees;
        public int? MaxAttendees
        {
            get => maxAttendees;
            set
            {
                if (value != maxAttendees)
                {
                    maxAttendees = value;
                    OnPropertyChanged(nameof(MaxAttendees));
                }
            }
        }

        private string date = string.Empty;
        public string Date
        {
            get => date;
            set
            {
                if (value != date)
                {
                    date = value;
                    OnPropertyChanged(nameof(Date));
                }
            }
        }

        private string startAt = string.Empty;
        public string StartAt
        {
            get => startAt;
            set
            {
                if (value != startAt)
                {
                    startAt = value;
                    OnPropertyChanged(nameof(StartAt));
                }
            }
        }

        public int Duration { get; set; } = 90; 

        private string? roomId;
        public string? RoomId
        {
            get => roomId;
            set
            {
                if (value != roomId)
                {
                    roomId = value;
                    OnPropertyChanged(nameof(RoomId));
                }
            }
        }

        private int durationWeeks;
        public int DurationWeeks
        {
            get => durationWeeks;
            set
            {
                if (value != durationWeeks)
                {
                    durationWeeks = value;
                    OnPropertyChanged(nameof(DurationWeeks));
                }
            }
        }

        private List<DayOfWeek> days = new();
        public List<DayOfWeek> Days
        {
            get => days;
            set
            {
                if (value != days)
                {
                    days = value;
                    OnPropertyChanged(nameof(Days));
                }
            }
        }

        public CourseDTO()
        {
            Id = 0;
            ProfessorId = string.Empty;
            Language = Language.English;
            Level = LanguageLevel.A1;
            IsLive = true;
            MaxAttendees = 0;
            Date = string.Empty;
            StartAt = string.Empty;
            RoomId = string.Empty;
            DurationWeeks = 1;
            Days = new List<DayOfWeek>();
        }

        public CourseDTO(Course course)
        {
            Id = course.Id;
            ProfessorId = course.ProfessorId;
            Language = course.Language;
            Level = course.Level;
            IsLive = course.IsLive;
            MaxAttendees = course.MaxAttendees;
            Date = course.Date ?? string.Empty;
            StartAt = course.StartAt ?? string.Empty;
            RoomId = course.RoomId ?? string.Empty;
            DurationWeeks = course.DurationWeeks;
            Days = course.Days ?? new List<DayOfWeek>();
            Duration = 90;
        }

        public Course ToCourse()
        {
            return new Course(Id, ProfessorId, Language, Level, IsLive, MaxAttendees, Date, StartAt, Days, DurationWeeks, RoomId);
        }

       
        public string Error => string.Empty;

        private readonly Regex _timeRegex = new Regex(@"^([01]\d|2[0-3]):[0-5]\d$"); // HH:mm

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(MaxAttendees):
                        if (IsLive && (!MaxAttendees.HasValue || MaxAttendees.Value <= 0))
                            return "For live courses, MaxAttendees must be greater than 0.";
                        break;

                    case nameof(Date):
                        if (string.IsNullOrWhiteSpace(Date))
                            return "Please select start date.";
                        if (!DateTime.TryParse(Date, out _))
                            return "Invalid date format.";
                        break;

                    case nameof(StartAt):
                        if (string.IsNullOrWhiteSpace(StartAt))
                            return "Please select start time.";
                        if (!_timeRegex.IsMatch(StartAt))
                            return "Invalid time format (HH:mm).";
                        break;

                    case nameof(DurationWeeks):
                        if (DurationWeeks <= 0)
                            return "DurationWeeks must be greater than 0.";
                        break;

                    case nameof(Days):
                        if (Days == null || Days.Count == 0)
                            return "Please select at least one day.";
                        break;
                }
                return string.Empty;
            }
        }

        private readonly string[] _validatedProperties =
            { nameof(Language), nameof(Level), nameof(Date), nameof(StartAt), nameof(DurationWeeks), nameof(Days) };

        public bool IsValid
        {
            get
            {
                foreach (var prop in _validatedProperties)
                {
                    if (this[prop] != string.Empty)
                    {
                        Debug.WriteLine($"Validation failed for {prop}: {this[prop]}");
                        return false;
                    }
                }
                return true;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
