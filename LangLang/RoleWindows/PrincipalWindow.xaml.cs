using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LangLang.Controller;
using LangLang.DTO;
using LangLang.Model;
using LangLang.Model.DAO;
using LangLang.Model.Enums;

namespace LangLang.RoleWindows
{
    /// <summary>
    /// Interaction logic for PrincipalWindow.xaml
    /// </summary>
    public partial class PrincipalWindow : Window
    {
        private ProfessorsController professorsController;
        private bool isEditMode = false;
        private ProfessorDTO currentProfessor = null;
        public ProfessorDTO Professor { get; set; }

        // Language selection properties
        public ObservableCollection<Language> AvailableLanguages { get; set; }
        public ObservableCollection<LanguageLevel> AvailableLevels { get; set; }
        public Dictionary<Language, LanguageLevel> ProfessorLanguages { get; set; }
        public ObservableCollection<string> ProfessorLanguagesDisplay { get; set; }

        public PrincipalWindow(ProfessorsController professorsController)
        {
            InitializeComponent();
            DataContext = this;

            this.professorsController = professorsController;
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
            var professors = professorsController.GetAllProfessors();
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

        private void AddProfessor_Click(object sender, RoutedEventArgs e)
        {
            isEditMode = false;
            currentProfessor = null;

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
                isEditMode = true;
                currentProfessor = new ProfessorDTO(selectedProfessor);

                // Reset collections
                ProfessorLanguages.Clear();
                ProfessorLanguagesDisplay.Clear();

                // Fill DTO with existing values
                Professor = currentProfessor;

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
            // TODO: implement delete functionality
            MessageBox.Show("Delete teacher not implemented yet.");
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
            if (Professor.IsValid)
            {
                if (!isEditMode && ProfessorLanguages.Count == 0)
                {
                    MessageBox.Show("Please add at least one language for the professor.",
                                    "No languages selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (isEditMode)
                {
                    // Editing existing professor
                    var updatedProfessor = Professor.ToProfessor();

                    // Keep original registration date & languages
                    updatedProfessor.DateOfRegistration = currentProfessor.DateOfRegistration;
                    updatedProfessor.Languages = currentProfessor.Languages;

                    professorsController.Update(updatedProfessor);

                    MessageBox.Show("Professor successfully updated!",
                                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Creating new professor
                    var professor = Professor.ToProfessor();

                    if (professorsController.GetProfessorById(professor.Email) != null)
                    {
                        MessageBox.Show("An account with this email already exists. Please use a different email.",
                                        "Email already exists", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    professor.DateOfRegistration = DateTime.Now.ToString("yyyy-MM-dd");
                    professor.Languages = new Dictionary<Language, LanguageLevel>(ProfessorLanguages);

                    professorsController.Add(professor);

                    MessageBox.Show("Professor successfully registered!",
                                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                AddEditProfessorOverlay.Visibility = Visibility.Collapsed;
                LoadProfessors();
            }
            else
            {
                MessageBox.Show("Please correct validation errors before saving.",
                                "Invalid input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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
    }
}