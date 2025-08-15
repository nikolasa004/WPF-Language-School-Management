using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LangLang
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //temporary variables for user credentials
        private string studentEmail = "student@gmail.com";
        private string studentPassword = "student123";

        private string teacherEmail = "teacher@gmail.com";
        private string teacherPassword = "teacher123";

        private string principalEmail = "principal@gmail.com";
        private string principalPassword = "principal123";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenLoginPanel(object sender, RoutedEventArgs e)
        {
            MainPanel.Visibility = Visibility.Collapsed;
            LoginPanel.Visibility = Visibility.Visible;
        }

        private void OpenRegisterPanel(object sender, RoutedEventArgs e)
        {
            RegisterPanel.Visibility = Visibility.Visible;
            MainPanel.Visibility = Visibility.Collapsed;
        }

        private void CloseLoginPanel(object sender, RoutedEventArgs e)
        {
            LoginPanel.Visibility = Visibility.Collapsed;
            MainPanel.Visibility = Visibility.Visible;
        }

        private void CloseRegisterPanel(object sender, RoutedEventArgs e)
        {
            RegisterPanel.Visibility = Visibility.Collapsed;
            MainPanel.Visibility = Visibility.Visible;
        }

        private void TryLogin(object sender, RoutedEventArgs e)
        {
            string email = EmailBoxLogin.Text.Trim();
            string password = PasswordBoxLogin.Password.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both email and password.");
                return;
            }

            // Check credentials against temporary variables
            if (email == studentEmail && password == studentPassword)
            {
                // Navigate to Student Dashboard
            }
            else if (email == teacherEmail && password == teacherPassword)
            {
                // Navigate to Teacher Dashboard
            }
            else if (email == principalEmail && password == principalPassword)
            {
                // Navigate to Principal Dashboard
            }
            else
            {
                MessageBox.Show("Invalid email or password. Please try again.");
            }
        }

        private void TryRegister(object sender, RoutedEventArgs e)
        {
            string errorMessage = "";

            string name = FirstNameBox.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                errorMessage += "Please enter your first name.\n";
            }
            else if (name.Any(c => !char.IsLetter(c)))
            {
                errorMessage += "First name can only contain letters.\n";
            }

            string surname = LastNameBox.Text.Trim();
            if (string.IsNullOrEmpty(surname))
            {
                errorMessage += "Please enter your last name.\n";
            }
            else if (surname.Any(c => !char.IsLetter(c)))
            {
                errorMessage += "Last name can only contain letters.\n";
            }

            bool isMale = MaleRadioButton.IsChecked == true;

            DateTime birthDate;
            if (BirthDatePicker.SelectedDate.HasValue)
            {
                birthDate = BirthDatePicker.SelectedDate.Value;
                if (birthDate > DateTime.Now)
                {
                    errorMessage += "Birth date cannot be in the future.\n";
                }
                else if (birthDate < DateTime.Now.AddYears(-120))
                {
                    errorMessage += "Birth date cannot be more than 120 years ago.\n";
                }
                else if (birthDate > DateTime.Now.AddYears(-18))
                {
                    errorMessage += "You must be at least 18 years old to register.\n";
                }
            }
            else
            {
                errorMessage += "Please select your birth date.\n";
            }

            string phoneNumber = PhoneBox.Text.Trim();
            if (string.IsNullOrEmpty(phoneNumber))
            {
                errorMessage += "Please enter your phone number.\n";
            }
            else if (phoneNumber.Any(c => !char.IsDigit(c) && c != '+'))
            {
                errorMessage += "Phone number can only contain digits and '+' sign.\n";
            }

            string email = EmailBoxRegister.Text.Trim();
            if (string.IsNullOrEmpty(email))
            {
                errorMessage += "Please enter your email.\n";
            }
            else if (!email.Contains("@") || !email.Contains("."))
            {
                errorMessage += "Please enter a valid email address.\n";
            }

            string password = PasswordBoxRegister.Password.Trim();
            if (string.IsNullOrEmpty(password))
            {
                errorMessage += "Please enter your password.\n";
            }

            string educationLevel = EducationComboBox.Text.Trim();
            if (string.IsNullOrEmpty(educationLevel))
            {
                errorMessage += "Please select your education level.\n";
            }

            if (errorMessage != "")
            {
                MessageBox.Show("Registration failed:\n" + errorMessage);
                return;
            }
            else
            {
                MessageBox.Show("You have successfully registered!\n" + "Welcome " + name + " " + surname + "!");
                //Here new user data should be saved to the file
            }
        }
    }
}
