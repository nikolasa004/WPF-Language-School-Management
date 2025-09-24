using LangLang.Controller.Messages;
using LangLang.Model.Messages;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LangLang.View
{
    public partial class MessagesInboxWindow : Window
    {
        private List<Message> messages;
        private readonly MessagesController _messagesController;

        public MessagesInboxWindow(string studentId, MessagesController messagesController)
        {
            _messagesController = messagesController;
            InitializeComponent();
            messages = _messagesController.GetStudentMessagesById(studentId);
            LoadMessages();
        }
        private void LoadMessages()
        {
            MessageListPanel.Children.Clear();
            int row = 0;
            if (messages.Count == 0)
            {
                TextBlock noMessagesText = new TextBlock{Text = "No messages available.",Foreground = Brushes.White,FontFamily = new FontFamily("Script MT Bold"),FontSize = 20,VerticalAlignment = VerticalAlignment.Center,HorizontalAlignment = HorizontalAlignment.Center,Margin = new Thickness(10)};
                MessageListPanel.Children.Add(noMessagesText);
                return;
            }
            foreach (Message message in messages)
            {
                MessageListPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                Button btn = MakeButton(message);

                Grid.SetRow(btn, row);
                MessageListPanel.Children.Add(btn);
                row++;
            }
        }
        private Button MakeButton(Message message)
        {
            Button btn=new Button { Height = 60, Margin = new Thickness(0, 0, 0, 5), Padding = new Thickness(0), Style = (Style)FindResource("FancyButtonStyle") };
            btn.Click += MessageRow_Click;
            btn.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/GUIResources/topBarButtonBackground.png")) };
            btn.Tag = message;

            StackPanel btnContent = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Left };
            Ellipse indicator = new Ellipse { Width = 16, Height = 16, Fill = message.IsRead ? Brushes.Green : Brushes.Red, Margin = new Thickness(10, 0, 10, 0), VerticalAlignment = VerticalAlignment.Center };
            TextBlock title = new TextBlock { Text = message.Title, Foreground = Brushes.White, FontFamily = new FontFamily("Script MT Bold"), FontSize = 20, VerticalAlignment = VerticalAlignment.Center };
            btnContent.Children.Add(indicator);
            btnContent.Children.Add(title);
            btn.Content = btnContent;

            return btn;
        }

        private void BackToInbox_Click(object sender, RoutedEventArgs e)
        {
            MessageDetailPanel.Visibility = Visibility.Collapsed;
            MessageListPanel.Visibility = Visibility.Visible;
        }

        private void MessageRow_Click(object sender, RoutedEventArgs e)
        {
            Button btn=ReadMessage(sender);

            Message message = (Message)btn.Tag;
            DetailTitle.Text = message.Title;
            DetailSender.Text = message.SenderId;
            DetailBody.Text = message.Body;
            DetailDate.Text = message.Date;
            MessageListPanel.Visibility = Visibility.Collapsed;
            MessageDetailPanel.Visibility = Visibility.Visible;
            message.IsRead = true;
            _messagesController.StudentUpdateReadStatus(message.Id);
        }
        private Button ReadMessage(object sender)
        {
            Button btn=(Button)sender;
            StackPanel content = (StackPanel)btn.Content;
            Ellipse indicator = (Ellipse)content.Children[0];
            indicator.Fill = Brushes.Green;
            return btn;
        }

        private void DeleteRead_Click(object sender, RoutedEventArgs e)
        {
            for (int i = MessageListPanel.Children.Count - 1; i >= 0; i--)
            {
                if (MessageListPanel.Children[i] is Button btn)
                {
                    StackPanel stackPanel = (StackPanel)btn.Content;
                    Ellipse indicator = (Ellipse)stackPanel.Children[0];
                    if (indicator.Fill == Brushes.Green)
                    {
                        MessageListPanel.Children.RemoveAt(i);
                        Message message = (Message)btn.Tag;
                        _messagesController.StudentDelete(message.Id);
                    }
                }
            }
        }
        private void MarkAllAsRead_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in MessageListPanel.Children)
            {
                if (child is Button btn)
                {
                    StackPanel stackPanel = (StackPanel)btn.Content;
                    Ellipse indicator = (Ellipse)stackPanel.Children[0];
                    indicator.Fill = Brushes.Green;
                    Message message = (Message)btn.Tag;
                    if (!message.IsRead)
                    {
                        message.IsRead = true;
                        _messagesController.StudentUpdateReadStatus(message.Id);
                    }
                }
            }
        }
    }
}