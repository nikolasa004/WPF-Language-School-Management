using LangLang.Controller.Users;
using System.Windows;

namespace LangLang.View.StudentViewModules
{
    public partial class StudentFinishedExamsWindow : Window
    {
        private readonly StudentsController studentsController;
        private readonly string studentId;

        public StudentFinishedExamsWindow(StudentsController studentsController, string studentId)
        {
            this.studentsController = studentsController;
            this.studentId = studentId;

            InitializeComponent();
            LoadFinishedExams();
        }

        private void LoadFinishedExams()
        {
            // vraća List<Tuple<Language, LanguageLevel>>
            var finishedExams = studentsController.GetFinishedExams(studentId);
            ExamsGrid.ItemsSource = finishedExams;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
