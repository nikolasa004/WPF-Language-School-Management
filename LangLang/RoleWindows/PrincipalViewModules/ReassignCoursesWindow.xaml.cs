using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using LangLang.Controller;
using LangLang.Model;
using LangLang.Model.Enums;

namespace LangLang.RoleWindows
{
    public partial class ReassignCoursesWindow : Window
    {
        private readonly CourseController courseController;
        private readonly ProfessorsController professorsController;
        private readonly Professor professorToDelete;

        public string HeaderText { get; set; }
        public ObservableCollection<CourseAssignment> CourseAssignments { get; set; }
        public ObservableCollection<ProfessorOption> AvailableProfessors { get; set; }

        public bool WasReassigned { get; private set; } = false;

        public ReassignCoursesWindow(
            List<Course> ongoingCourses,
            Professor professorToDelete,
            CourseController courseController,
            ProfessorsController professorsController)
        {
            InitializeComponent();
            DataContext = this;

            this.courseController = courseController;
            this.professorsController = professorsController;
            this.professorToDelete = professorToDelete;

            HeaderText = $"Reassign Courses for {professorToDelete.FirstName} {professorToDelete.LastName}";

            InitializeCollections(ongoingCourses);
        }

        private void InitializeCollections(List<Course> ongoingCourses)
        {
            CourseAssignments = new ObservableCollection<CourseAssignment>();
            AvailableProfessors = new ObservableCollection<ProfessorOption>();

            // Get all professors except the one being deleted
            var allProfessors = professorsController.GetAllProfessors()
                .Where(p => p.Email != professorToDelete.Email)
                .ToList();

            // Add "Select Professor" option at the beginning
            AvailableProfessors.Add(new ProfessorOption { Email = "", DisplayName = "-- Select Professor --" });

            // Create course assignments
            foreach (var course in ongoingCourses)
            {
                var assignment = new CourseAssignment
                {
                    Course = course,
                    DaysDisplay = string.Join(", ", course.Days?.Select(d => d.ToString().Substring(0, 3)) ?? new List<string>()),
                    AvailableProfessorsForCourse = GetCompatibleProfessors(course, allProfessors)
                };

                CourseAssignments.Add(assignment);
            }
        }

        private List<ProfessorOption> GetCompatibleProfessors(Course course, List<Professor> allProfessors)
        {
            var compatibleProfessors = new List<ProfessorOption>
            {
                new ProfessorOption { Email = "", DisplayName = "-- Select Professor --" }
            };

            foreach (var professor in allProfessors)
            {
                // Check if professor teaches the required language and level
                if (professor.Languages != null &&
                    professor.Languages.ContainsKey(course.Language) &&
                    professor.Languages[course.Language] >= course.Level)
                {
                    // Check for schedule conflicts
                    if (!HasScheduleConflict(professor, course))
                    {
                        compatibleProfessors.Add(new ProfessorOption
                        {
                            Email = professor.Email,
                            DisplayName = $"{professor.FirstName} {professor.LastName} ({professor.Email})"
                        });

                        // Also add to global available professors if not already there
                        if (!AvailableProfessors.Any(p => p.Email == professor.Email))
                        {
                            AvailableProfessors.Add(new ProfessorOption
                            {
                                Email = professor.Email,
                                DisplayName = $"{professor.FirstName} {professor.LastName} ({professor.Email})"
                            });
                        }
                    }
                }
            }

            return compatibleProfessors;
        }

        private bool HasScheduleConflict(Professor professor, Course newCourse)
        {
            try
            {
                var existingCourses = courseController.GetMyCourses(professor.Email);

                // Create a temporary course with the professor to check for conflicts
                var tempCourse = new Course
                {
                    Id = -1, // Temporary ID
                    ProfessorId = professor.Email,
                    Language = newCourse.Language,
                    Level = newCourse.Level,
                    Date = newCourse.Date,
                    StartAt = newCourse.StartAt,
                    Duration = newCourse.Duration,
                    DurationWeeks = newCourse.DurationWeeks,
                    Days = newCourse.Days,
                    MaxAttendees = newCourse.MaxAttendees,
                    IsLive = newCourse.IsLive
                };

                // Use the course controller's overlap checking logic
                // We'll simulate this by checking manually since the controller's method is private
                return CheckForOverlapManually(existingCourses, tempCourse);
            }
            catch
            {
                // If there's an error checking conflicts, assume there is a conflict to be safe
                return true;
            }
        }

        private bool CheckForOverlapManually(List<Course> existingCourses, Course newCourse)
        {
            var newSessions = ExpandToSessions(newCourse);

            foreach (var existing in existingCourses)
            {
                var existingSessions = ExpandToSessions(existing);

                foreach (var newSession in newSessions)
                {
                    foreach (var existingSession in existingSessions)
                    {
                        bool overlap = newSession.start < existingSession.end &&
                                     existingSession.start < newSession.end;
                        if (overlap)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static List<(DateTime start, DateTime end)> ExpandToSessions(Course c)
        {
            var sessions = new List<(DateTime, DateTime)>();

            if (!DateTime.TryParseExact(c.Date ?? "", "dd.MM.yyyy",
                System.Globalization.CultureInfo.GetCultureInfo("sr-RS"),
                System.Globalization.DateTimeStyles.None, out var baseDate))
                return sessions;

            if (!TimeSpan.TryParseExact(c.StartAt ?? "", "hh\\:mm",
                System.Globalization.CultureInfo.InvariantCulture, out var ts))
                return sessions;

            for (int week = 0; week < c.DurationWeeks; week++)
            {
                foreach (var day in c.Days ?? new List<DayOfWeek>())
                {
                    var sessionDate = GetNextWeekday(baseDate.AddDays(7 * week), day);
                    var start = sessionDate.Date.Add(ts);
                    var end = start.AddMinutes(c.Duration);
                    sessions.Add((start, end));
                }
            }
            return sessions;
        }

        private static DateTime GetNextWeekday(DateTime start, DayOfWeek targetDay)
        {
            int daysToAdd = ((int)targetDay - (int)start.DayOfWeek + 7) % 7;
            return start.AddDays(daysToAdd);
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            var unassignedCourses = new List<Course>();

            // Check if all courses have professors assigned
            foreach (var assignment in CourseAssignments)
            {
                if (assignment.SelectedProfessor == null ||
                    string.IsNullOrEmpty(assignment.SelectedProfessor.Email))
                {
                    unassignedCourses.Add(assignment.Course);
                }
            }

            if (unassignedCourses.Any())
            {
                var courseList = string.Join("\n", unassignedCourses.Select(c =>
                    $"Course ID {c.Id} ({c.Language} {c.Level})"));

                MessageBox.Show($"Please assign professors to all courses before proceeding.\n\n" +
                               $"Unassigned courses:\n{courseList}",
                               "Incomplete Assignment",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                return;
            }

            // Perform the reassignments
            var errors = new List<string>();

            foreach (var assignment in CourseAssignments)
            {
                try
                {
                    var course = assignment.Course;
                    course.ProfessorId = assignment.SelectedProfessor.Email;

                    var updated = courseController.ForceUpdate(course);
                    if (updated == null)
                    {
                        errors.Add($"Failed to update Course ID {course.Id}");
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Course ID {assignment.Course.Id}: {ex.Message}");
                }
            }

            if (errors.Any())
            {
                var errorMessage = "Some courses could not be reassigned:\n\n" +
                                 string.Join("\n", errors);
                MessageBox.Show(errorMessage, "Reassignment Errors",
                               MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show($"Successfully reassigned {CourseAssignments.Count} courses!",
                           "Success", MessageBoxButton.OK, MessageBoxImage.Information);

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

    // Helper classes
    public class CourseAssignment : INotifyPropertyChanged
    {
        public Course Course { get; set; }
        public string DaysDisplay { get; set; }
        public List<ProfessorOption> AvailableProfessorsForCourse { get; set; }

        private ProfessorOption _selectedProfessor;
        public ProfessorOption SelectedProfessor
        {
            get => _selectedProfessor;
            set
            {
                _selectedProfessor = value;
                OnPropertyChanged(nameof(SelectedProfessor));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ProfessorOption
    {
        public string Email { get; set; }
        public string DisplayName { get; set; }
    }
}