using LangLang.Controller.Messages;
using LangLang.Model.Messages;
using System.Windows;

namespace LangLang.View
{
    public partial class MessageSendWindow : Window
    {
        private readonly MessagesController messagesController;
        private readonly string recipientId;
        private readonly string senderId;
        private readonly string title;
        private readonly string type;

        public MessageSendWindow(string senderId, string title, string label, string recipientId, MessagesController messagesController, string type)
        {
            this.messagesController = messagesController;
            this.senderId = senderId;
            this.recipientId = recipientId;
            this.title = title;
            this.type = type;
            InitializeComponent();
            MessageLabel.Content = label;
        }
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBoxResult.Yes;

            if (type == "professor")
            {
                result = MessageBox.Show(
                    "Are you sure you want to unenroll from this course? Without a good reason, you will receive a penalty point.",
                    "Unenroll from course",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
            }
            else if (type == "student")
            {
                result = MessageBox.Show(
                    "Are you sure you want to send this message?",
                    "Send Message",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
            }

            if (result == MessageBoxResult.Yes)
            {
                Message message = new Message
                {
                    RecipientId = recipientId,
                    SenderId = senderId,
                    Title = title,
                    Body = MessageTextBox.Text
                };

                if (type == "student")
                    messagesController.StudentAdd(message);
                else
                    messagesController.ProfessorAdd(message);
            }

            Close();
        }
    }
}