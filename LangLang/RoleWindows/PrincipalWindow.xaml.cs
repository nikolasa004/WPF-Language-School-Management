using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LangLang.Controller;
using LangLang.DTO;
using LangLang.Model;
using LangLang.Model.DAO;
using LangLang.Model.Enums;

namespace LangLang.RoleWindows
{
    public partial class PrincipalWindow : Window
    {
        private ProfessorsController _professorsController;
        private bool _isEditMode = false;
        private ProfessorDTO _currentProfessor = null;
        public ProfessorDTO Professor { get; set; }

        // Language selection properties
        public ObservableCollection<Language> AvailableLanguages { get; set; }
        public ObservableCollection<LanguageLevel> AvailableLevels { get; set; }
        public Dictionary<Language, LanguageLevel> ProfessorLanguages { get; set; }
        public ObservableCollection<string> ProfessorLanguagesDisplay { get; set; }

        public class DuplicateEmailException : Exception { }


        public PrincipalWindow(ProfessorsController _professorsController)
        {
            InitializeComponent();
            DataContext = this;

            this._professorsController = _professorsController;
            Professor = new ProfessorDTO();

            InitializeLanguageCollections();
            LoadProfessors();
        }

        private void InitializeLanguageCollections()
        {
            AvailableLanguages = new ObservableCollection<Language>(Enum.GetValues(typeof(Language)).Cast<Language>());
            AvailableLevels = new ObservableCollection<LanguageLevel>(Enum.GetValues(typeof(LanguageLevel)).Cast<LanguageLevel>());
            ProfessorLanguages = new Dictionary<Language, LanguageLevel>();
            ProfessorLanguagesDisplay = new ObservableCollection<string>();
        }
        private void LoadProfessors()
        {
            var professors = _professorsController.GetAllProfessors();
            Console.WriteLine($"Loaded {professors.Count} professors"); // Debug output
            ProfessorsDataGrid.ItemsSource = professors;

            // Force refresh
            ProfessorsDataGrid.Items.Refresh();
        }

        // === Professors actions ===
        private void ProfessorsButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }


        private void ReassignCourses_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Course reassignment functionality has been integrated into the delete professor workflow.",
                            "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CancelCourseReassignment_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Course reassignment cancelled.",
                            "Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddProfessor_Click(object sender, RoutedEventArgs e)
        {
            _isEditMode = false;
            _currentProfessor = null;

            Professor = new ProfessorDTO();
            ProfessorLanguages.Clear();
            ProfessorLanguagesDisplay.Clear();

            LanguagesSection.Visibility = Visibility.Visible;

            DataContext = null;
            DataContext = this;

            AddEditProfessorOverlay.Visibility = Visibility.Visible;
        }

        private void UpdateProfessor_Click(object sender, RoutedEventArgs e)
        {
            if (ProfessorsDataGrid.SelectedItem is Professor selectedProfessor)
            {
                _isEditMode = true;
                _currentProfessor = new ProfessorDTO(selectedProfessor);

                // Reset collections
                ProfessorLanguages.Clear();
                ProfessorLanguagesDisplay.Clear();

                // Fill DTO with existing values
                Professor = _currentProfessor;

                // Hide languages section in edit mode
                LanguagesSection.Visibility = Visibility.Collapsed;

                DataContext = null;
                DataContext = this;

                AddEditProfessorOverlay.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show("Please select a professor to edit.", "No selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void DeleteProfessor_Click(object sender, RoutedEventArgs e)
        {
            if (ProfessorsDataGrid.SelectedItem is not Professor selectedProfessor)
            {
                MessageBox.Show("Please select a professor to delete.",
                                "No selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete professor {selectedProfessor.FirstName} {selectedProfessor.LastName}?",
                "Confirm delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            var isAssigned = HandleProfessorCourses(selectedProfessor);
            HandleProfessorExams(selectedProfessor);
            
            if (!isAssigned) return;

            _professorsController.Delete(selectedProfessor.Email);

            MessageBox.Show("Professor successfully deleted!",
                            "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            LoadProfessors();
        }

        private bool HandleProfessorCourses(Professor professor)
        {
            var courseController = new CourseController();
            var professorExams = courseController.GetMyCourses(professor.Email);

            var ongoingCourses = new List<Course>();

            foreach (var exam in professorExams)
            {
                try
                {
                    DateTime examDate = ParseDate(exam.Date);
                    if (examDate > DateTime.Now)
                    {
                        courseController.ForceDelete(exam.Id);
                    }
                    else
                    {
                        ongoingCourses.Add(exam);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error while processing exam {exam.Id}: {ex.Message}",
                                    "Exam error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Console.WriteLine(ex.ToString());
                }
            }
            
            // Handle ongoing courses - need reassignment
            if (ongoingCourses.Any())
            {
                var reassignWindow = new ReassignCoursesWindow(
                    ongoingCourses,
                    professor,
                    courseController,
                    this._professorsController);

                var result = reassignWindow.ShowDialog();

                if (result != true || !reassignWindow.WasReassigned)
                {
                    MessageBox.Show("Course reassignment was cancelled. The professor cannot be deleted while having ongoing courses.",
                                    "Deletion Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; // Exit the deletion process
                }
            }

            return true; 
        }


        private void HandleProfessorExams(Professor professor)
        {
            var examsController = new ExamTermsController();
            var professorExams = examsController.GetMyExamTerms(professor.Email);

            foreach (var exam in professorExams)
            {
                try
                {
                    DateTime examDate = ParseDate(exam.Date);
                    if (examDate > DateTime.Now)
                    {
                        examsController.ForceDelete(exam.Id);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error while processing exam {exam.Id}: {ex.Message}",
                                    "Exam error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Console.WriteLine(ex.ToString());
                }
            }
        }


        /// <summary>
        /// Helper za parsiranje datuma iz stringa "yyyy-MM-dd"
        /// </summary>
        private DateTime ParseDate(string startAt)
        {
            return DateTime.TryParse(startAt, out var parsed) ? parsed : DateTime.MinValue;
        }



        private void AddLanguage_Click(object sender, RoutedEventArgs e)
        {
            var selectedLanguage = LanguageComboBox.SelectedItem as Language?;
            var selectedLevel = LanguageLevelComboBox.SelectedItem as LanguageLevel?;

            if (selectedLanguage.HasValue && selectedLevel.HasValue)
            {
                // Check if this language is already added
                if (ProfessorLanguages.ContainsKey(selectedLanguage.Value))
                {
                    MessageBox.Show("This language is already added. Please select a different language.",
                                    "Language already exists", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                ProfessorLanguages.Add(selectedLanguage.Value, selectedLevel.Value);
                ProfessorLanguagesDisplay.Add($"{selectedLanguage.Value} - {selectedLevel.Value}");

                // Clear the selection
                LanguageComboBox.SelectedIndex = -1;
                LanguageLevelComboBox.SelectedIndex = -1;
                
            }
            else
            {
                MessageBox.Show("Please select both a language and a level.",
                                "Invalid selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RemoveLanguage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string languageDisplay)
            {
                var parts = languageDisplay.Split(new[] { " - " }, StringSplitOptions.None);
                if (parts.Length == 2 && Enum.TryParse<Language>(parts[0], out var language))
                {
                    ProfessorLanguages.Remove(language);
                    ProfessorLanguagesDisplay.Remove(languageDisplay);
                }
            }
        }

        private void SaveProfessor_Click(object sender, RoutedEventArgs e)
        {
            if (!Professor.IsValid)
            {
                ShowValidationError();
                return;
            }

            if (!_isEditMode && ProfessorLanguages.Count == 0)
            {
                ShowNoLanguagesWarning();
                return;
            }

            try
            {
                if (_isEditMode)
                {
                    UpdateExistingProfessor();
                }
                else
                {
                    CreateNewProfessor();
                }

                CloseOverlayAndRefresh();
                ShowSuccessMessage();
            }
            catch (DuplicateEmailException ex)
            {
                ShowDuplicateEmailWarning();
            }
        }

        private void ShowValidationError()
        {
            MessageBox.Show("Please correct validation errors before saving.",
                           "Invalid input", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void ShowNoLanguagesWarning()
        {
            MessageBox.Show("Please add at least one language for the professor.",
                           "No languages selected", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void ShowDuplicateEmailWarning()
        {
            MessageBox.Show("An account with this email already exists. Please use a different email.",
                           "Email already exists", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void ShowSuccessMessage()
        {
            string message = _isEditMode ? "Professor successfully updated!" : "Professor successfully registered!";
            MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CloseOverlayAndRefresh()
        {
            AddEditProfessorOverlay.Visibility = Visibility.Collapsed;
            LoadProfessors();
        }

        private void UpdateExistingProfessor()
        {
            var updatedProfessor = Professor.ToProfessor();

            if (EmailExists(updatedProfessor.Email))
            {
                throw new DuplicateEmailException();
            }

            updatedProfessor.DateOfRegistration = _currentProfessor.DateOfRegistration;
            updatedProfessor.Languages = _currentProfessor.Languages;

            _professorsController.Update(updatedProfessor);
        }

        private void CreateNewProfessor()
        {
            var professor = Professor.ToProfessor();

            if (EmailExists(professor.Email))
            {
                throw new DuplicateEmailException();
            }

            professor.DateOfRegistration = DateTime.Now.ToString("yyyy-MM-dd");
            professor.Languages = new Dictionary<Language, LanguageLevel>(ProfessorLanguages);

            _professorsController.Add(professor);
        }

        private bool EmailExists(string email)
        {
            return _professorsController.GetProfessorById(email) != null;
        }
        

        private void CancelProfessor_Click(object sender, RoutedEventArgs e)
        {
            AddEditProfessorOverlay.Visibility = Visibility.Collapsed;
        }

        // === Courses ===
        private void CoursesButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void SmartAssignProfessor_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Smart assign professor not implemented yet.");
        }

        // === Exams ===
        private void ExamsButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void SmartAssignExaminer_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Smart assign examiner not implemented yet.");
        }

        // === Reports ===
        private void ReportsButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void ReportPenalties_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Penalty report not implemented yet.");
        }

        private void ReportCourses_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Course report not implemented yet.");
        }

        private void ReportExams_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Exam report not implemented yet.");
        }

        private void ReportLanguages_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Language report not implemented yet.");
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get all professors
                var professors = _professorsController.GetAllProfessors();

                // --- Collect filters ---
                var selectedLanguage = ExamLanguageCombo.SelectedItem as Language?;
                var selectedLevel = ExamLevelCombo.SelectedItem as LanguageLevel?;
                var fromDate = ExamFromDate.SelectedDate;
                var toDate = ExamToDate.SelectedDate;

                // --- Apply filters ---
                var filtered = professors.AsEnumerable();

                if (selectedLanguage.HasValue)
                {
                    filtered = filtered.Where(p =>
                        p.Languages != null && p.Languages.ContainsKey(selectedLanguage.Value));
                }

                if (selectedLevel.HasValue)
                {
                    filtered = filtered.Where(p =>
                        p.Languages != null &&
                        p.Languages.Values.Contains(selectedLevel.Value));
                }

                if (fromDate.HasValue)
                {
                    filtered = filtered.Where(p =>
                    {
                        if (DateTime.TryParse(p.DateOfRegistration, out var regDate))
                            return regDate >= fromDate.Value;
                        return false;
                    });
                }

                if (toDate.HasValue)
                {
                    filtered = filtered.Where(p =>
                    {
                        if (DateTime.TryParse(p.DateOfRegistration, out var regDate))
                            return regDate <= toDate.Value;
                        return false;
                    });
                }

                // --- Update DataGrid ---
                ProfessorsDataGrid.ItemsSource = filtered.ToList();
                ProfessorsDataGrid.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during search: {ex.Message}",
                                "Search Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveFilters_Click(object sender, RoutedEventArgs e)
        {
            // Clear selections
            ExamLanguageCombo.SelectedIndex = -1;
            ExamLevelCombo.SelectedIndex = -1;
            ExamFromDate.SelectedDate = null;
            ExamToDate.SelectedDate = null;

            // Reload full professor list
            LoadProfessors();
        }

    }
}