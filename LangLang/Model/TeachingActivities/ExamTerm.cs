using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using LangLang.Model.Enums;
using LangLang.Storage.Serialization;

namespace LangLang.Model.TeachingActivities
{
    public class ExamTerm: ProfessorTeachingActivity, ISerializable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public new int Id { get; set; }
        public bool IsLocked { get; set; }
        public bool IsArchived { get; set; }

        [JsonIgnore]
        public ICollection<ExamResult> ExamResults { get; set; }
        public ExamTerm() : base() 
        {
            Id = -1;
            IsLive = true;
            Duration = 240;
            
        }

        public ExamTerm(int id,string professorId,Language language,LanguageLevel level,int maxAttendees,string? date,string? startAt,string? roomId): base(id, professorId, language, level, true, maxAttendees, date, startAt, 240, roomId)
        {
            IsLocked = false;
            IsArchived = false;
        }

        public override string ToString()
        {
            return base.ToString();
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
            ExamTerm? exam = JsonSerializer.Deserialize<ExamTerm>(value);
            if (exam != null)
            {
                Id = exam.Id;
                ProfessorId = exam.ProfessorId;
                Language = exam.Language;
                Level = exam.Level;
                IsLive = true;
                Duration = 240;
                MaxAttendees = exam.MaxAttendees;
                Date = exam.Date;
                StartAt = exam.StartAt;
                RoomId = exam.RoomId;
                IsLocked = exam.IsLocked;
                IsArchived = exam.IsArchived;
            }
        }
    }
}
