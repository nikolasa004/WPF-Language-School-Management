using LangLang.Model.Enums;
using LangLang.Storage.Serialization;
using System.Text.Json;

namespace LangLang.Model.Requests
{
    public class ExamEnrollment : Request, ISerializable
    {
        public ExamEnrollment() : base()
        {
        }

        public ExamEnrollment(int id, string studentID, int teachingActivityID, string requestDate, RequestStatus status)
            : base(id, studentID, teachingActivityID, requestDate, status)
        {
        }

        public string ToJSON()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(this, options);
        }

        public void FromJSON(string value)
        {
            ExamEnrollment? request = JsonSerializer.Deserialize<ExamEnrollment>(value);
            if (request != null)
            {
                ID = request.ID;
                StudentID = request.StudentID;
                TeachingActivityID = request.TeachingActivityID;
                RequestDate = request.RequestDate;
                Status = request.Status;
            }
        }
    }
}
