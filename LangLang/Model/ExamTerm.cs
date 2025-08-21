using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LangLang.Model.Enums;
using System.Windows;
using System.Xml.Serialization;
using System.Text.Json;
using LangLang.Storage.Serialization;

namespace LangLang.Model
{
    public class ExamTerm: ProfessorTeachingActivity, ISerializable
    {
        public ExamTerm() : base() 
        {
            
            IsLive = true;
            Duration = 240;
            
        }

        public ExamTerm(int id,string professorId,Language language,LanguageLevel level,int maxAttendees,string? date,string? startAt,string? roomId)
        {
        
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
            }
        }
    }
}
