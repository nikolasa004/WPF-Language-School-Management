using LangLang.Storage.Serialization;
using System.Text.Json;

namespace LangLang.Model.Messages
{
    public class Message:ISerializable
    {
        public int Id { get; set; }
        public string RecipientId { get; set; }
        public string SenderId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Date { get; set; }
        public bool IsRead { get; set; }

        public Message()
        {
            RecipientId = string.Empty;
            SenderId = string.Empty;
            Title = string.Empty;
            Body = string.Empty;
            Date = string.Empty;
        }
        public Message(int id, string recipientId, string senderId, string title, string body, string date, bool isRead)
        {
            Id = id;
            RecipientId = recipientId;
            SenderId = senderId;
            Title = title;
            Body = body;
            Date = date;
            IsRead = isRead;
        }

        public override string ToString()
        {
            return "Id: " + Id + "\n" + "RecipientId: " + RecipientId + "\n" +"SenderId: " + SenderId + "\n" +"Title: " + Title + "\n" +"Body: " + Body + "\n" +"Date: " + Date+ "\n" + "IsRead: " + IsRead + "\n";
        }

        public string ToJSON()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(this, options);
        }
        public void FromJSON(string value)
        {
            Message? message = JsonSerializer.Deserialize<Message>(value);
            if (message != null)
            {
                Id = message.Id;
                RecipientId = message.RecipientId;
                SenderId = message.SenderId;
                Title = message.Title;
                Body = message.Body;
                Date = message.Date;
                IsRead = message.IsRead;
            }
        }
    }
}