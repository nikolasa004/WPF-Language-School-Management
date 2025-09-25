    using System;
using System.Windows;
using LangLang.DTO;
using LangLang.Model.Enums;

namespace LangLang.Controller.Services
{
    public class ExamButtonInfo
    {
        public ExamButtonState State { get; set; }
        public string Content { get; set; }
        public bool IsEnabled { get; set; }
        public string BackgroundColor { get; set; }

        public ExamButtonInfo(ExamButtonState state, string content, bool isEnabled, string backgroundColor = "#FF000000")
        {
            State = state;
            Content = content;
            IsEnabled = isEnabled;
            BackgroundColor = backgroundColor;
        }
    }

    public class ExamButtonService
    {
        public ExamButtonInfo GetButtonInfo(ExamTermDTO exam)
        {
            if (exam?.Date == null || exam?.StartAt == null) return new ExamButtonInfo(ExamButtonState.NotAvailable, "Not Available", false, "#FF808080");

            var now = DateTime.Now;
            var examDateTime = CombineDateAndTime(exam.Date, exam.StartAt);
            var examEndDateTime = examDateTime.AddMinutes(exam.Duration);

            var timeDifference = examDateTime - now;
            var daysDifference = timeDifference.TotalDays;

            if (now > examEndDateTime && exam.IsLocked == true)
            {
                return new ExamButtonInfo(ExamButtonState.Finish, "Finish", true);
            }

            // Exam is currently in progress (between start and end time)
            if (now >= examDateTime && now <= examEndDateTime) return new ExamButtonInfo(ExamButtonState.Open, "Open", true);

            // 5 days or less before exam
            if (daysDifference <= 5 && daysDifference > 0 && exam.IsLocked == false) return new ExamButtonInfo(ExamButtonState.Start, "Start", true);

            // More than 5 days before exam
            return new ExamButtonInfo(ExamButtonState.NotAvailable, "Not Available", false, "#FF808080");
        }

        private DateTime CombineDateAndTime(object date, object startAt)
        {
            DateTime examDate;
            TimeSpan examTime;

            // Handle different possible types for date
            if (date is DateTime dt)
                examDate = dt.Date;
            else if (DateTime.TryParse(date?.ToString(), out dt))
                examDate = dt.Date;
            else
                throw new ArgumentException("Invalid date format");

            // Handle different possible types for startAt
            if (startAt is TimeSpan ts)
                examTime = ts;
            else if (TimeSpan.TryParse(startAt?.ToString(), out ts))
                examTime = ts;
            else if (DateTime.TryParse(startAt?.ToString(), out var timeDateTime))
                examTime = timeDateTime.TimeOfDay;
            else
                throw new ArgumentException("Invalid time format");

            return examDate.Add(examTime);
        }

        public ExamButtonState GetButtonState(ExamTermDTO exam)
        {
            return GetButtonInfo(exam).State;
        }
    }
}
