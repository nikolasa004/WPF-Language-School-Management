
namespace LangLang.Model.Enums
{
    public enum ExamButtonState
    {
        NotAvailable,
        Start,      // 5 days or less before exam
        Open,       // Same day and time
        Finish      // Past exam
    }

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
}
