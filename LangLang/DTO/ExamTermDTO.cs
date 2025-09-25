using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using LangLang.Model.Enums;
using LangLang.Model.TeachingActivities;

namespace LangLang.DTO
{
    public class ExamTermDTO : INotifyPropertyChanged, IDataErrorInfo
    {
        public int Id { get; set; }
        public string ProfessorId { get; set; } = string.Empty;

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

        public bool IsArchived { get; set; }

        private bool isLocked;
        public bool IsLocked
        {
            get => isLocked;
            set
            {
                if (value != isLocked)
                {
                    isLocked = value;
                    OnPropertyChanged(nameof(IsLocked));
                }
            }
        }

        private int maxAttendees;
        public int MaxAttendees
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
        public int Duration { get; set; } = 240;
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
        public ExamTermDTO()
        {
            Id = 0;
            ProfessorId = string.Empty;
            IsLive = true;
            MaxAttendees = 0;
            Date = string.Empty;
            StartAt = string.Empty;
            Duration = 240;
            RoomId = string.Empty;
            IsLocked = false;
            IsArchived = false;
        }
        public ExamTermDTO(ExamTerm examTerm)
        {
            Id = examTerm.Id;
            ProfessorId = examTerm.ProfessorId;
            Language = examTerm.Language;
            Level = examTerm.Level;
            IsLive = examTerm.IsLive;
            MaxAttendees = examTerm.MaxAttendees ?? 0;
            Date = examTerm.Date ?? string.Empty;
            StartAt = examTerm.StartAt ?? string.Empty;
            Duration = examTerm.Duration = 240;
            RoomId = examTerm.RoomId ?? string.Empty;
            IsLocked = examTerm.IsLocked;
            IsArchived = examTerm.IsArchived;
        }
        public ExamTerm ToExamTerm()
        {
            return new ExamTerm(Id, ProfessorId, Language, Level, MaxAttendees, Date, StartAt, RoomId)
            {
                IsLive = IsLive,
                Duration = Duration,
                IsLocked = IsLocked,
                IsArchived = IsArchived

            };
        }
        public string Error => string.Empty;

        private readonly Regex _timeRegex = new Regex(@"^([01]\d|2[0-3]):[0-5]\d$", RegexOptions.Compiled);

        public string this[string columnName]
        {
            get
            {
                var sr = new CultureInfo("sr-RS");
                switch (columnName)
                {
                    case nameof(MaxAttendees):
                        if (MaxAttendees <= 0)
                            return "Max attendees must be greater than 0.";
                        break;

                    case nameof(Date):
                        if (string.IsNullOrWhiteSpace(Date))
                            return "Please select exam date.";
                        if (!DateTime.TryParseExact(Date, "dd.MM.yyyy", sr, DateTimeStyles.None, out _))
                            return "Invalid date format (dd.MM.yyyy).";
                        break;

                    case nameof(StartAt):
                        if (string.IsNullOrWhiteSpace(StartAt))
                            return "Please select start time.";
                        if (!_timeRegex.IsMatch(StartAt))
                            return "Invalid time format (HH:mm).";
                        break;
                }
                return string.Empty;
            }
        }
        private readonly string[] _validatedProperties =
            { nameof(Language), nameof(Level), nameof(MaxAttendees), nameof(Date), nameof(StartAt) };

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
        public bool TryGetStart(out DateTime start)
        {
            start = default;
            var sr = new CultureInfo("sr-RS");
            if (!DateTime.TryParseExact(Date, "dd.MM.yyyy", sr, DateTimeStyles.None, out var d))
                return false;
            if (!TimeSpan.TryParseExact(StartAt, @"hh\:mm", CultureInfo.InvariantCulture, out var t))
                return false;

            start = d.Date.Add(t);
            return true;
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
