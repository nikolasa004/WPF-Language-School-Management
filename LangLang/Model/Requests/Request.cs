using LangLang.Model.Enums;
using System;

namespace LangLang.Model.Requests
{
    public abstract class Request
    {
        public int ID { get; set; }
        public string StudentID { get; set; }
        public int TeachingActivityID { get; set; }
        public string RequestDate { get; set; }
        public RequestStatus Status { get; set; }

        public Request()
        {
            StudentID = string.Empty;
            RequestDate = string.Empty;
        }
        public Request(int id, string studentID, int teachingActivityID, string requestDate, RequestStatus status) 
        {
            ID = id;
            StudentID = studentID;
            TeachingActivityID = teachingActivityID;
            RequestDate = requestDate;
            Status = status;
        }

        public override string ToString()
        {
            return "Id: "+ID+"\n"+"StudentId: "+StudentID+"\n"+"TeachingActivityId: "+ TeachingActivityID +"\n"+"RequestDate: "+RequestDate+"\n"+"Status: "+Status;
        }
    }
}