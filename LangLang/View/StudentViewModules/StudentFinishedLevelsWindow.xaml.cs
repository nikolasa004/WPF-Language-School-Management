using LangLang.Controller.Users;
using LangLang.Model.Enums;
using System;
using System.Collections.Generic;
using System.Windows;

namespace LangLang.View.StudentViewModules
{
    public partial class StudentFinishedLevelsWindow : Window
    {
        StudentsController studentsController;
        private string studentId;

        public StudentFinishedLevelsWindow(StudentsController studentsController, string studentId)
        {
            this.studentsController = studentsController;
            this.studentId = studentId;
            InitializeComponent();

            LoadFinishedLevels("courses");
        }

        private void LoadFinishedLevels(string type)
        {
            if(type == "courses")
            {
                LoadFinishedCourses();
            }
        }
        private void LoadFinishedCourses()
        {
            List<Tuple<Language, LanguageLevel>> finishedCourses = studentsController.GetFinishedCourses(studentId);
            LanguagesGrid.ItemsSource = finishedCourses;
        }
    }
}
