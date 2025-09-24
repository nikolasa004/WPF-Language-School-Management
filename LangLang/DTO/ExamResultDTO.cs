using System.ComponentModel;
using LangLang.Model.TeachingActivities;

namespace LangLang.DTO
{
    public class ExamResultDTO : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public ExamTermDTO Exam { get; set; }
        public StudentDTO Student { get; set; }

        private int readingPoints;
        public int ReadingPoints
        {
            get => readingPoints;
            set
            {
                if (readingPoints != value)
                {
                    readingPoints = value;
                    OnPropertyChanged(nameof(ReadingPoints));
                    OnPropertyChanged(nameof(TotalPoints));
                    OnPropertyChanged(nameof(Passed));
                }
            }
        }

        private int writingPoints;
        public int WritingPoints
        {
            get => writingPoints;
            set
            {
                if (writingPoints != value)
                {
                    writingPoints = value;
                    OnPropertyChanged(nameof(WritingPoints));
                    OnPropertyChanged(nameof(TotalPoints));
                    OnPropertyChanged(nameof(Passed));
                }
            }
        }

        private int listeningPoints;
        public int ListeningPoints
        {
            get => listeningPoints;
            set
            {
                if (listeningPoints != value)
                {
                    listeningPoints = value;
                    OnPropertyChanged(nameof(ListeningPoints));
                    OnPropertyChanged(nameof(TotalPoints));
                    OnPropertyChanged(nameof(Passed));
                }
            }
        }

        private int speakingPoints;
        public int SpeakingPoints
        {
            get => speakingPoints;
            set
            {
                if (speakingPoints != value)
                {
                    speakingPoints = value;
                    OnPropertyChanged(nameof(SpeakingPoints));
                    OnPropertyChanged(nameof(TotalPoints));
                    OnPropertyChanged(nameof(Passed));
                }
            }
        }

        public int TotalPoints => ReadingPoints + WritingPoints + ListeningPoints + SpeakingPoints;

        public bool Passed =>
            ReadingPoints >= 30 &&
            WritingPoints >= 30 &&
            ListeningPoints >= 20 &&
            SpeakingPoints >= 25 &&
            TotalPoints >= 160;

        public ExamResultDTO() { }

        public ExamResultDTO(ExamResult examResult, ExamTermDTO exam, StudentDTO student)
        {
            Id = examResult.Id;
            Exam = exam;
            Student = student;
            ReadingPoints = examResult.ReadingPoints;
            WritingPoints = examResult.WritingPoints;
            ListeningPoints = examResult.ListeningPoints;
            SpeakingPoints = examResult.SpeakingPoints;
        }
        public ExamResult ToExamResult()
        {
            return new ExamResult(Exam.Id, Student.Email, ReadingPoints, WritingPoints, ListeningPoints, SpeakingPoints)
            {
                Id = Id
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
