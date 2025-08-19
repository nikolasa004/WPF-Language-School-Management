using LangLang.RoleWindows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LangLang.Controller;
using LangLang.DTO;
using LangLang.Model;

namespace LangLang
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public StudentDTO Student { get; set; }

        private StudentsController studentsController;

        public ProfessorDTO Professor { get; set; }

        private ProfessorsController professorsController;

        //temporary variables for user credentials
       // private string teacherEmail = "teacher@gmail.com";
       // private string teacherPassword = "teacher123";

        private string principalEmail = "principal@gmail.com";
        private string principalPassword = "principal123";
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Student = new StudentDTO();
            studentsController = new StudentsController();
            Professor = new ProfessorDTO();
            professorsController = new ProfessorsController();
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
                MessageBox.Show("Please enter both email and password.", "Empty input",MessageBoxButton.OK,MessageBoxImage.Warning);
                return;
            }

            // Check credentials against temporary variables(students work now)
            bool errorOccurred = false;
            if (studentsController.GetStudentById(email) is var student && student!=null)
            {
                if (student.Password == password)
                {
                    OpenNewWindow(new StudentWindow(student,studentsController));
                }
                else
                {
                    errorOccurred = true;
                }
            }
            else if (professorsController.GetProfessorById(email) is var professor && professor != null)
            {
                if (professor.Password == password)
                {
                    OpenNewWindow(new ProfessorWindow(professor, professorsController));
                }
                else
                {
                    errorOccurred = true;
                }
            }
            else if (email == principalEmail && password == principalPassword)
            {
                OpenNewWindow(new PrincipalWindow());
            }
            else
            {
                errorOccurred = true;
            }

            if (errorOccurred)
                MessageBox.Show("Invalid email or password. Please try again.", "Login error",MessageBoxButton.OK,MessageBoxImage.Error);
        }
        private void OpenNewWindow(Window newWindow)
        {
            bool isFullScreen = WindowState == WindowState.Maximized;

            newWindow.Top = Top;
            newWindow.Left = Left;
            newWindow.Width = Width;
            newWindow.Height = Height;

            if (isFullScreen)
            {
                newWindow.WindowState = WindowState.Maximized;
            }

            newWindow.Show();
            Close();
        }

        private void TryRegister_Click(object sender, RoutedEventArgs e)
        {
            if (Student.IsValid)
            {
                Student student=Student.ToStudent();
                if (studentsController.GetStudentById(student.Email)!=null)
                {
                    MessageBox.Show("An account with this email already exists. Please log in or use a different email to create an account.", "Email already exists",MessageBoxButton.OK,MessageBoxImage.Warning);
                    return;
                }
                student.DateOfRegistration = DateTime.Now.ToString("yyyy-MM-dd");
                studentsController.Add(student);

                MessageBox.Show("Registration successful! You can now log in.", "Registration successful", MessageBoxButton.OK, MessageBoxImage.Information);

                CloseRegisterPanel_Click(sender, e);
            }
        }
    }
}
