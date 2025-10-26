using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using LangLang.Storage.Serialization;

namespace LangLang.Model.TeachingActivities
{
    public class ExamResult : ISerializable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ExamId { get; set; }
        public string StudentId { get; set; }

        public int ReadingPoints { get; set; }
        public int WritingPoints { get; set; }
        public int ListeningPoints { get; set; }
        public int SpeakingPoints { get; set; }
        public int TotalPoints => ReadingPoints + WritingPoints + ListeningPoints + SpeakingPoints;

        public bool Passed =>
            ReadingPoints >= 30 &&    
            WritingPoints >= 30 &&
            ListeningPoints >= 20 &&  
            SpeakingPoints >= 25 &&   
            TotalPoints >= 160;


        [JsonIgnore]
        [ForeignKey("ExamId")]
        public ExamTerm? ExamTerm { get; set; }

        public ExamResult() { 
            Id  = -1;
            ExamId = -1;
            StudentId = "";
            ReadingPoints = -1;
            WritingPoints = -1;
            ListeningPoints = -1;
            SpeakingPoints = -1;

        }

        public ExamResult(int examId, string studentId, int reading, int writing, int listening, int speaking)
        {
            ExamId = examId;
            StudentId = studentId;
            ReadingPoints = reading;
            WritingPoints = writing;
            ListeningPoints = listening;
            SpeakingPoints = speaking;
        }

        public string ToJSON()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            return JsonSerializer.Serialize(this, options);
        }

        public void FromJSON(string value)
        {
            var result = JsonSerializer.Deserialize<ExamResult>(value);
            if (result != null)
            {
                Id = result.Id;
                ExamId = result.ExamId;
                StudentId = result.StudentId;
                ReadingPoints = result.ReadingPoints;
                WritingPoints = result.WritingPoints;
                ListeningPoints = result.ListeningPoints;
                SpeakingPoints = result.SpeakingPoints;
            }
        }
    }
}
