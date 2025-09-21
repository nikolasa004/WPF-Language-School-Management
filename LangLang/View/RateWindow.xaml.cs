using LangLang.Controller.Users;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LangLang.View
{
    public partial class RateWindow : Window
    {
        private ProfessorsController professorsController;

        private string recipientId;
        private string type;

        private Button[] ratingButtons = new Button[10];
        private int currentRating = 0;

        public RateWindow(string recipientId, string type, ProfessorsController professorsController)
        {
            this.recipientId = recipientId;
            this.type = type;
            this.professorsController= professorsController;
            InitializeComponent();
            if (type == "professor")
            {
                TopLabel.Content = "You finished your course!Please rate your professor.";
            }
            InitializeRatingButtons();
        }

        private void InitializeRatingButtons()
        {
            for (int i = 0; i < 10; i++)
            {
                var stack = new StackPanel() { Orientation = Orientation.Vertical, Margin = new Thickness(5, 0, 5, 0) };

                var label = new Label(){Content = (i + 1).ToString(),FontFamily = new FontFamily("Script MT Bold"),Foreground = Brushes.White,HorizontalAlignment = HorizontalAlignment.Center,FontSize = 20};
                stack.Children.Add(label);

                var btn = new Button(){Width = 60,Height = 60,Tag = i + 1,Background = Brushes.Red,BorderBrush = Brushes.Gray,BorderThickness = new Thickness(2),Content = "★",FontSize = 30,FontFamily = new FontFamily("Script MT Bold"),Foreground = Brushes.White};

                btn.Click += RatingButton_Click;
                stack.Children.Add(btn);

                ratingButtons[i] = btn;
                StarsPanel.Items.Add(stack);
            }
        }

        private void RatingButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedBtn)
            {
                int rating = (int)clickedBtn.Tag;
                currentRating = rating;

                for (int i = 0; i < 10; i++)
                {
                    ratingButtons[i].Background = i < rating ? Brushes.Green : Brushes.Red;
                }
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentRating == 0)
            {
                MessageBox.Show("Please select a rating before submitting.", "No Rating Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            MessageBoxResult result=MessageBox.Show("Are you sure you want to rate "+recipientId+" with grade "+currentRating+"?" ,"Submit Rating",MessageBoxButton.YesNo,MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                if (type == "professor")
                {
                    professorsController.AddRating(recipientId, currentRating);
                }
                Close();
            }
        }
    }
}