using LangLang.Model.Messages;
using LangLang.Observer;
using LangLang.Storage;
using System.Collections.Generic;
using System.Linq;

namespace LangLang.Model.DAO.Messages
{
    class MessagesDAO
    {
        private readonly List<Message> messages = new List<Message>();
        private readonly Dictionary<string,List<Message>> _messages = new Dictionary<string, List<Message>>();
        private readonly Storage<Message> _storage;

        public Subject MessageSubject;

        public MessagesDAO(string userType)
        {
            _storage = new Storage<Message>(userType+"-messages.json");
            messages = _storage.Load();
            _messages = messages.GroupBy(m => m.RecipientId).ToDictionary(g => g.Key, g => g.ToList());
            MessageSubject = new Subject();
        }

        public Message AddMessage(Message message)
        {
            int nextId = messages.Count > 0 ? messages[^1].Id + 1 : 1;
            message.Id = nextId;
            message.IsRead = false;
            message.Date = System.DateTime.Now.ToString("yyyy-MM-dd");
            if (!_messages.ContainsKey(message.RecipientId))
            {
                _messages[message.RecipientId] = new List<Message>();
            }
            _messages[message.RecipientId].Add(message);
            messages.Add(message);
            _storage.Save(messages);
            MessageSubject.NotifyObservers();
            return message;
        }
        public Message? RemoveMessage(int id)
        {
            Message? message = GetMessageById(id);
            if (message == null) return null;

            messages.Remove(message);
            _messages[message.RecipientId].Remove(message);
            _storage.Save(messages);
            MessageSubject.NotifyObservers();
            return message;
        }
        public void RemoveMessagesByUser(string userId)
        {
            _messages.Remove(userId);
            messages.RemoveAll(m => m.RecipientId == userId);
            _storage.Save(messages);
            MessageSubject.NotifyObservers();
        }
        public void UpdateMessageReadStatus(int id)
        {
            Message? message = GetMessageById(id);
            if (message != null)
            {
                message.IsRead = true;
            }
            _storage.Save(messages);
        }

        public Message? GetMessageById(int id)
        {
            return messages.Find(m => m.Id == id);
        }
        public List<Message> GetMessagesByRecipientId(string recipientId)
        {
            return _messages.ContainsKey(recipientId) ? _messages[recipientId] : new List<Message>();
        }
        public List<Message> GetAllMessages()
        {
            return messages;
        }
    }
}