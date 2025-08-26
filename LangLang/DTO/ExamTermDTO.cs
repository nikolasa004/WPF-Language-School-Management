using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using LangLang.Model;
using LangLang.Model.Enums;
namespace LangLang.DTO
{
    public class ExamTermDTO : INotifyPropertyChanged, IDataErrorInfo
    {
        public int Id { get; set; }
        public string ProfessorId { get; set; }
        private Language language;
        public Language Language
        {
            get 
            { 
                return language;
            }
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
            get
            {
                return level;
            }
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
        private int maxAttendees;
        public int MaxAttendees
        {
            get
            {
                return maxAttendees;
            }
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
            get
            {
                return date;
            }
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
            get  
            {
                return startAt;

            }
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
            get 
            {return roomId;}
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
        }
        public ExamTermDTO(ExamTerm examTerm)
        {
            Id = examTerm.Id;
            ProfessorId = examTerm.ProfessorId;
            Language = examTerm.Language;
            Level = examTerm.Level;
            IsLive = true;             
            MaxAttendees = examTerm.MaxAttendees ?? 0;
            Date = examTerm.Date ?? string.Empty;
            StartAt = examTerm.StartAt ?? string.Empty;
            Duration = 240;
            RoomId = examTerm.RoomId ?? string.Empty;
        }
        public ExamTerm ToExamTerm()
            {
            return new ExamTerm(Id, ProfessorId, Language, Level, MaxAttendees, Date, StartAt, RoomId)
            {
                IsLive = IsLive,
                Duration = Duration
            };
        }
        public string Error => string.Empty;
        private readonly Regex _dateRegex = new Regex(@"^([01]\d|2[0-3]):[0-5]\d$"); // HH:mm
        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {

                    case nameof(MaxAttendees):
                        if (MaxAttendees <= 0)
                            return "Max attendees must be greater than 0.";
                        break;

                    case nameof(Date):
                        if (string.IsNullOrWhiteSpace(Date))
                            return "Please select exam date.";
                        if (!DateTime.TryParse(Date, out _))
                            return "Invalid date format.";
                        break;

                    case nameof(StartAt):
                        if (string.IsNullOrWhiteSpace(StartAt))
                            return "Please select start time.";
                        if (!_dateRegex.IsMatch(StartAt))
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
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}