using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LangLang.Controller.Users;
using LangLang.DTO;
using LangLang.Model.Enums;
using LangLang.Model.Users;

namespace LangLang.RoleWindows
{
    public class PrincipalProfessorView
    {
        private readonly ProfessorsController _professorsController;
        private readonly PrincipalProfessorManager _professorManager;
        private readonly DataGrid _professorsDataGrid;
        private readonly Window _parentWindow;

        private bool _isEditMode = false;
        private ProfessorDTO? _currentProfessor = null;

        // Properties for data binding
        public ProfessorDTO Professor { get; set; }
        public ObservableCollection<Language> AvailableLanguages { get; set; }
        public ObservableCollection<LanguageLevel> AvailableLevels { get; set; }
        public Dictionary<Language, LanguageLevel> ProfessorLanguages { get; set; }
        public ObservableCollection<string> ProfessorLanguagesDisplay { get; set; }

        public PrincipalProfessorView(ProfessorsController professorsController, DataGrid dataGrid, Window parentWindow)
        {
            _professorsController = professorsController;
            _professorManager = new PrincipalProfessorManager(professorsController);
            _professorsDataGrid = dataGrid;
            _parentWindow = parentWindow;

            Professor = new ProfessorDTO();
            
            AvailableLanguages = new ObservableCollection<Language>(Enum.GetValues(typeof(Language)).Cast<Language>());
            AvailableLevels = new ObservableCollection<LanguageLevel>(Enum.GetValues(typeof(LanguageLevel)).Cast<LanguageLevel>());
            ProfessorLanguages = new Dictionary<Language, LanguageLevel>();
            ProfessorLanguagesDisplay = new ObservableCollection<string>();
        }

        public void LoadProfessors()
        {
            var professors = _professorsController.GetAllProfessors();
            _professorsDataGrid.ItemsSource = professors;
            _professorsDataGrid.Items.Refresh();
        }

        public Professor? GetSelectedProfessor()
        {
            return _professorsDataGrid.SelectedItem as Professor;
        }

        public void ShowAddProfessorDialog(FrameworkElement overlay, FrameworkElement languagesSection)
        {
            _isEditMode = false;
            _currentProfessor = null;

            Professor = new ProfessorDTO();
            ProfessorLanguages.Clear();
            ProfessorLanguagesDisplay.Clear();

            languagesSection.Visibility = Visibility.Visible;

            RefreshDataContext();
            overlay.Visibility = Visibility.Visible;
        }

        public void ShowEditProfessorDialog(FrameworkElement overlay, FrameworkElement languagesSection)
        {
            var selectedProfessor = GetSelectedProfessor();
            if (selectedProfessor == null)
            {
                MessageBox.Show("Please select a professor to edit.", "No selection",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _isEditMode = true;
            _currentProfessor = new ProfessorDTO(selectedProfessor);

            ProfessorLanguages.Clear();
            ProfessorLanguagesDisplay.Clear();

            Professor = _currentProfessor;
            languagesSection.Visibility = Visibility.Collapsed;

            RefreshDataContext();
            overlay.Visibility = Visibility.Visible;
        }

        public void AddLanguage(ComboBox languageCombo, ComboBox levelCombo)
        {
            var selectedLanguage = languageCombo.SelectedItem as Language?;
            var selectedLevel = levelCombo.SelectedItem as LanguageLevel?;

            if (selectedLanguage.HasValue && selectedLevel.HasValue)
            {
                if (ProfessorLanguages.ContainsKey(selectedLanguage.Value))
                {
                    MessageBox.Show("This language is already added. Please select a different language.",
                                    "Language already exists", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                ProfessorLanguages.Add(selectedLanguage.Value, selectedLevel.Value);
                ProfessorLanguagesDisplay.Add($"{selectedLanguage.Value} - {selectedLevel.Value}");

                languageCombo.SelectedIndex = -1;
                levelCombo.SelectedIndex = -1;
            }
            else
            {
                MessageBox.Show("Please select both a language and a level.",
                                "Invalid selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void RemoveLanguage(object sender)
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

        public bool SaveProfessor()
        {
            try
            {
                _professorManager.SaveProfessor(Professor, ProfessorLanguages, _isEditMode, _currentProfessor);

                LoadProfessors();
                ShowSuccessMessage();
                return true;
            }
            catch (PrincipalProfessorManager.ValidationException ex)
            {
                MessageBox.Show(ex.Message, "Invalid input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            catch (PrincipalProfessorManager.DuplicateEmailException ex)
            {
                MessageBox.Show(ex.Message, "Email already exists", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void ShowSuccessMessage()
        {
            string message = _isEditMode ? "Professor successfully updated!" : "Professor successfully registered!";
            MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void SearchProfessors(ComboBox languageCombo, ComboBox levelCombo,
                                   DatePicker fromDate, DatePicker toDate)
        {
            try
            {
                var professors = _professorsController.GetAllProfessors();

                var selectedLanguage = languageCombo.SelectedItem as Language?;
                var selectedLevel = levelCombo.SelectedItem as LanguageLevel?;
                var fromDateValue = fromDate.SelectedDate;
                var toDateValue = toDate.SelectedDate;

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

                if (fromDateValue.HasValue)
                {
                    filtered = filtered.Where(p =>
                    {
                        if (DateTime.TryParse(p.DateOfRegistration, out var regDate))
                            return regDate >= fromDateValue.Value;
                        return false;
                    });
                }

                if (toDateValue.HasValue)
                {
                    filtered = filtered.Where(p =>
                    {
                        if (DateTime.TryParse(p.DateOfRegistration, out var regDate))
                            return regDate <= toDateValue.Value;
                        return false;
                    });
                }

                _professorsDataGrid.ItemsSource = filtered.ToList();
                _professorsDataGrid.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during search: {ex.Message}",
                                "Search Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void RemoveFilters(ComboBox languageCombo, ComboBox levelCombo,
                                DatePicker fromDate, DatePicker toDate)
        {
            languageCombo.SelectedIndex = -1;
            levelCombo.SelectedIndex = -1;
            fromDate.SelectedDate = null;
            toDate.SelectedDate = null;

            LoadProfessors();
        }

        private void RefreshDataContext()
        {
            _parentWindow.DataContext = null;
            _parentWindow.DataContext = this;
        }
    }
}