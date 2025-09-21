using LangLang.RoleWindows;
using System;
using System.Windows;
using LangLang.DTO;
using LangLang.Controller.Services;
using LangLang.Model.Users;
using LangLang.Controller.Users;

namespace LangLang
{
    public partial class MainWindow : Window
    {
        public StudentDTO Student { get; set; }
        private StudentsController studentsController;
        public ProfessorDTO Professor { get; set; }
        private ProfessorsController professorsController;

        private AuthenticationService authenticationController;

        private string principalEmail = "principal@gmail.com";
        private string principalPassword = "123";

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Student = new StudentDTO();
            studentsController = new StudentsController();
            Professor = new ProfessorDTO();
            professorsController = new ProfessorsController();
            authenticationController = new AuthenticationService(studentsController, professorsController, principalEmail, principalPassword);
            try
            {
                studentsController.ApplyPenaltyDecayForAll();
                // (opciono) Debug prikaz:
                // MessageBox.Show($"Penalty decay applied.", "Info");
            }
            catch (Exception ex)
            {
                // Ako nešto pođe po zlu, prikaži poruku umesto tihog pada
                MessageBox.Show(ex.Message, "Penalty decay error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        
        }



        private void OpenLoginPanel_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Visibility = Visibility.Collapsed;
            LoginPanel.Visibility = Visibility.Visible;
        }
        private void OpenRegisterPanel_Click(object sender, RoutedEventArgs e)
        {
            RegisterPanel.Visibility = Visibility.Visible;
            MainPanel.Visibility = Visibility.Collapsed;
        }
        private void CloseLoginPanel_Click(object sender, RoutedEventArgs e)
        {
            LoginPanel.Visibility = Visibility.Collapsed;
            MainPanel.Visibility = Visibility.Visible;
        }
        private void CloseRegisterPanel_Click(object sender, RoutedEventArgs e)
        {
            RegisterPanel.Visibility = Visibility.Collapsed;
            MainPanel.Visibility = Visibility.Visible;
        }

        private void TryLogin_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailBoxLogin.Text.Trim();
            string password = PasswordBoxLogin.Password.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both email and password.", "Empty input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            var user = authenticationController.Authenticate(email, password);
            if (user != null)
                OpenUserWindow(user);
            else
                MessageBox.Show("Invalid email or password. Please try again.", "Login error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void OpenUserWindow(object user)
        {
            Window newWindow = user switch
            {
                Student s => new StudentWindow(s, studentsController),
                Professor p => new ProfessorWindow(p, professorsController),
                string s when s == "Principal" => new PrincipalWindow(professorsController),
                _ => throw new InvalidOperationException("Unknown user type")
            };

            newWindow.Top = Top;
            newWindow.Left = Left;
            newWindow.Width = Width;
            newWindow.Height = Height;

            if (WindowState == WindowState.Maximized)
                newWindow.WindowState = WindowState.Maximized;

            newWindow.Show();
            Close();
        }

        private void TryRegister_Click(object sender, RoutedEventArgs e)
        {
            if (authenticationController.Register(Student, out string message))
            {
                MessageBox.Show(message, "Registration successful", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseRegisterPanel_Click(sender, e);
            }
            else
                MessageBox.Show(message, "Registration failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}