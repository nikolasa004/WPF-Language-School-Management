using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;
using LangLang.Controller;
using LangLang.Model;
using LangLang.Services;


namespace LangLang.RoleWindows
{
    public partial class ReassignCoursesWindow : Window
    {
        private readonly CourseReassignmentService _service;
        private readonly Professor _professorToDelete;

        public string HeaderText { get; set; }
        public ObservableCollection<CourseAssignment> CourseAssignments { get; set; }
        public bool WasReassigned { get; private set; } = false;

        public ReassignCoursesWindow(
            List<Course> ongoingCourses,
            Professor professorToDelete,
            CourseController courseController,
            ProfessorsController professorsController)
        {
            InitializeComponent();
            DataContext = this;

            _service = new CourseReassignmentService(courseController, professorsController);
            _professorToDelete = professorToDelete;

            HeaderText = $"Reassign Courses for {professorToDelete.FirstName} {professorToDelete.LastName}";

            var assignments = _service.BuildAssignments(ongoingCourses, professorToDelete);

            CourseAssignments = new ObservableCollection<CourseAssignment>(assignments);
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            var unassigned = _service.ValidateAssignments(CourseAssignments);

            if (unassigned.Any())
            {
                var courseList = string.Join("\n", unassigned.Select(c =>
                    $"Course ID {c.Id} ({c.Language} {c.Level})"));
                MessageBox.Show($"Please assign professors to all courses before proceeding.\n\n" +
                                $"Unassigned courses:\n{courseList}",
                                "Incomplete Assignment",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            var errors = _service.PerformReassignment(CourseAssignments);
            if (errors.Any())
            {
                MessageBox.Show("Some courses could not be reassigned:\n\n" +
                                string.Join("\n", errors),
                                "Reassignment Errors",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            MessageBox.Show($"Successfully reassigned {CourseAssignments.Count} courses!",
                            "Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

            WasReassigned = true;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

}