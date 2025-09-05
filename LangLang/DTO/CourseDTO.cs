using LangLang.Model;
using LangLang.Model.Enums;
using LangLang.DTO.Validators;
using System.ComponentModel;
using System.Collections.Generic;
using System;

namespace LangLang.DTO
{
    public class CourseDTO : INotifyPropertyChanged, IDataErrorInfo
    {
        private bool SetProperty<T>(ref T storage, T value, string propertyName)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private int id;
        public int Id { get => id; set => SetProperty(ref id, value, nameof(Id)); }

        private string professorId = string.Empty;
        public string ProfessorId { get => professorId; set => SetProperty(ref professorId, value, nameof(ProfessorId)); }

        private Language language;
        public Language Language { get => language; set => SetProperty(ref language, value, nameof(Language)); }

        private LanguageLevel level;
        public LanguageLevel Level { get => level; set => SetProperty(ref level, value, nameof(Level)); }

        private bool isLive = true;
        public bool IsLive { get => isLive; set => SetProperty(ref isLive, value, nameof(IsLive)); }

        private int? maxAttendees;
        public int? MaxAttendees { get => maxAttendees; set => SetProperty(ref maxAttendees, value, nameof(MaxAttendees)); }

        private string date = string.Empty;
        public string Date { get => date; set => SetProperty(ref date, value, nameof(Date)); }

        private string startAt = string.Empty;
        public string StartAt { get => startAt; set => SetProperty(ref startAt, value, nameof(StartAt)); }

        private int durationWeeks = 1;
        public int DurationWeeks { get => durationWeeks; set => SetProperty(ref durationWeeks, value, nameof(DurationWeeks)); }

        private List<DayOfWeek> days = new();
        public List<DayOfWeek> Days { get => days; set => SetProperty(ref days, value, nameof(Days)); }

        public string DaysFormatted => Days != null && Days.Count > 0 ? string.Join(", ", Days) : "N/A";

        private string? roomId;
        public string? RoomId { get => roomId; set => SetProperty(ref roomId, value, nameof(RoomId)); }

        public CourseDTO() { }

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
            DurationWeeks = course.DurationWeeks;
            Days = course.Days ?? new List<DayOfWeek>();
            RoomId = course.RoomId;
        }

        public Course ToCourse()
        {
            return new Course(Id, ProfessorId, Language, Level, IsLive, MaxAttendees, Date, StartAt, Days, DurationWeeks, RoomId);
        }

        private readonly CourseDTOValidator validator = new CourseDTOValidator();
        public string this[string columnName] => validator.Validate(this, columnName);
        public bool IsValid => validator.IsValid(this);
        public string Error => string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
