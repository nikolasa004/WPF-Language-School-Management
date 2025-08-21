using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LangLang.Model.Enums;

namespace LangLang.Model
{
    public class ProfessorTeachingActivity
    {
        public int Id { get; set; }
        public required string ProfessorId { get; set; }

        public Language Language { get; set; }
        public LanguageLevel Level { get; set; }

        public bool IsLive { get; set; }                    // Exam: true; Course: zavisi
        public int? MaxAttendees { get; set; }              // Exam: required; Course-live: required

        public string? Date { get; set; }

        public string? StartAt { get; set; }
        public int Duration { get; set; }              // Exam: 4h; Course: 90min
        public string? RoomId { get; set; }

        public ProfessorTeachingActivity()
        {
            Id = 0;
            ProfessorId = string.Empty;
            IsLive = false;
            MaxAttendees = 0;
            Date = string.Empty;
            StartAt = string.Empty;
            Duration = 0;
            RoomId = string.Empty;
        }

        public ProfessorTeachingActivity(int id, string professorId, Language language, LanguageLevel level, bool isLive,int? maxAttendees,string? date,string? startAt,int duration,string? roomId)
        {
            Id = id;
            ProfessorId = professorId;
            Language = language;
            Level = level;
            IsLive = isLive;
            MaxAttendees = maxAttendees;
            Date = date;
            StartAt = startAt;
            Duration = duration;
            RoomId = roomId;
        }

        public override string ToString()
        {
            return "Professor Teaching Activity | " +
                   $"Id: {Id}, ProfessorId: {ProfessorId}, Language: {Language}, Level: {Level}, " +
                   $"IsLive: {IsLive}, MaxAttendees: {MaxAttendees}, Date: {Date}, " +
                   $"StartAt: {StartAt}, Duration: {Duration} minutes, RoomId: {RoomId}";
        }
    }
}
