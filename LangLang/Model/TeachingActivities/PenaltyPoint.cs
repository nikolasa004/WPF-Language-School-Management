using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using LangLang.Model.Users;
using LangLang.Storage.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LangLang.Model.TeachingActivities
{
    public class PenaltyPoint:ISerializable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string StudentId { get; set; }
        public int CourseId { get; set; }


        public PenaltyPoint()
        {
            Id = -1;
            StudentId = string.Empty;
            CourseId = -1;
        }
        public PenaltyPoint(string studentId, int courseId)
        {
           
            StudentId = studentId;
            CourseId = courseId;

        }

        public string ToJSON()
        {
            var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
            return System.Text.Json.JsonSerializer.Serialize(this, options);
        }
        public void FromJSON(string value)
        {
            PenaltyPoint? penaltyPoint = System.Text.Json.JsonSerializer.Deserialize<PenaltyPoint>(value);
            if (penaltyPoint != null)
            {
                Id = penaltyPoint.Id;
                StudentId = penaltyPoint.StudentId;
                CourseId = penaltyPoint.CourseId;
            }
        }
    }
}