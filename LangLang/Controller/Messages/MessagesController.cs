using LangLang.Model.DAO.Messages;
using LangLang.Model.Messages;
using System.Collections.Generic;

namespace LangLang.Controller.Messages
{
    public class MessagesController
    {
        private readonly MessagesDAO _studentMessages;
        private readonly MessagesDAO _professorMessages;

        public MessagesController()
        {
            _studentMessages = new MessagesDAO("student");
            _professorMessages = new MessagesDAO("professor");
        }

        public void StudentAdd(Message message)
        {
            _studentMessages.AddMessage(message);
        }
        public void StudentDelete(int messageId)
        {
            _studentMessages.RemoveMessage(messageId);
        }
        public void StudentUpdateReadStatus(int messageId)
        {
            _studentMessages.UpdateMessageReadStatus(messageId);
        }

        public void ProfessorAdd(Message message)
        {
            _professorMessages.AddMessage(message);
        }
        public void ProfessorDelete(int messageId)
        {
            _professorMessages.RemoveMessage(messageId);
        }
        public void ProfessorUpdateReadStatus(int messageId)
        {
            _professorMessages.UpdateMessageReadStatus(messageId);
        }

        public List<Message>? GetStudentMessagesById(string email)
        {
            return _studentMessages.GetMessagesByRecipientId(email);
        }
    }
}
