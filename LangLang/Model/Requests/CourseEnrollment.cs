using LangLang.Model.Enums;
using LangLang.Storage.Serialization;
using System;
using System.Text.Json;

namespace LangLang.Model.Requests
{
    public class CourseEnrollment: Request, ISerializable
    {
        public CourseEnrollment() : base()
        {
        }
        public CourseEnrollment(int id, string studentID, int teachingActivityID, string requestDate, RequestStatus status) : base(id, studentID, teachingActivityID, requestDate, status)
        {
        }

        public string ToJSON()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(this, options);
        }
        public void FromJSON(string value)
        {
            CourseEnrollment? request = JsonSerializer.Deserialize<CourseEnrollment>(value);
            if (request != null)
            {
                ID=request.ID;
                StudentID=request.StudentID;
                TeachingActivityID=request.TeachingActivityID;
                RequestDate=request.RequestDate;
                Status=request.Status;
            }
        }
    }
}