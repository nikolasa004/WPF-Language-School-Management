using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using LangLang.Controller.TeachingActivities;
using LangLang.DTO;
using LangLang.Model.Enums;
using LangLang.Model.TeachingActivities;

namespace LangLang.RoleWindows.ProfessorExamModules
{
    public class ProfessorManageExams
    {
        private readonly string _professorId;
        private readonly ExamTermsController _examController;
        private readonly ProfessorViewExams _view;

        // ADD overlay
        private readonly Grid _addOverlay;
        private readonly ComboBox _addLangCombo, _addLevelCombo;
        private readonly DatePicker _addDatePicker;
        private readonly TextBox _addTimeBox, _addMaxBox, _addRoomBox;

        // EDIT overlay
        private readonly Grid _editOverlay;
        private readonly ComboBox _editLangCombo, _editLevelCombo;
        private readonly DatePicker _editDatePicker;
        private readonly TextBox _editTimeBox, _editMaxBox, _editRoomBox;

        private ExamTerm? _editingExam;

        // DTO instance za Add/Edit tokove
        private ExamTermDTO? _newExamDto;
        private ExamTermDTO? _editExamDto;

        public ProfessorManageExams(
            string professorId,
            ExamTermsController examController,
            ProfessorViewExams view,
            // add
            Grid addOverlay, ComboBox addLangCombo, ComboBox addLevelCombo, DatePicker addDatePicker, TextBox addTimeBox, TextBox addMaxBox, TextBox addRoomBox,
            // edit
            Grid editOverlay, ComboBox editLangCombo, ComboBox editLevelCombo, DatePicker editDatePicker, TextBox editTimeBox, TextBox editMaxBox, TextBox editRoomBox)
        {
            _professorId = professorId;
            _examController = examController;
            _view = view;

            _addOverlay = addOverlay;
            _addLangCombo = addLangCombo; _addLevelCombo = addLevelCombo;
            _addDatePicker = addDatePicker; _addTimeBox = addTimeBox; _addMaxBox = addMaxBox; _addRoomBox = addRoomBox;

            _editOverlay = editOverlay;
            _editLangCombo = editLangCombo; _editLevelCombo = editLevelCombo;
            _editDatePicker = editDatePicker; _editTimeBox = editTimeBox; _editMaxBox = editMaxBox; _editRoomBox = editRoomBox;
        }

        private void InitAddOverlay(Window hostWindow)
        {
            if (_addLangCombo != null && _addLangCombo.Items.Count == 0)
                _addLangCombo.ItemsSource = Enum.GetValues(typeof(Language));
            if (_addLevelCombo != null && _addLevelCombo.Items.Count == 0)
                _addLevelCombo.ItemsSource = Enum.GetValues(typeof(LanguageLevel));

            if (_addDatePicker != null)
            {
                _addDatePicker.Language = hostWindow.Language;
                _addDatePicker.SelectedDateFormat = DatePickerFormat.Short;
            }

            var sr = new CultureInfo("sr-RS");
            _newExamDto = new ExamTermDTO
            {
                ProfessorId = _professorId,
                Language = Language.English,
                Level = LanguageLevel.A1,
                IsLive = true,
                MaxAttendees = 10,
                Date = DateTime.Now.Date.AddDays(1).ToString("dd.MM.yyyy", sr),
                StartAt = "10:00",
                Duration = 240,
                RoomId = string.Empty
            };

            _addLangCombo.SelectedItem = _newExamDto.Language;
            _addLevelCombo.SelectedItem = _newExamDto.Level;
            _addDatePicker.SelectedDate = DateTime.ParseExact(_newExamDto.Date, "dd.MM.yyyy", sr);
            _addTimeBox.Text = _newExamDto.StartAt;
            _addMaxBox.Text = _newExamDto.MaxAttendees.ToString();
            _addRoomBox.Text = _newExamDto.RoomId ?? "";
        }

        public void OpenAddOverlay(Window hostWindow)
        {
            InitAddOverlay(hostWindow);
            _addOverlay.Visibility = Visibility.Visible;
        }

        public void CloseAddExamOverlay() => _addOverlay.Visibility = Visibility.Collapsed;

        public void SaveNewExam(Window hostWindow)
        {
            if (_newExamDto is null)
            {
                MessageBox.Show("Internal error: DTO not initialized.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_addLangCombo.SelectedItem is Language lang) _newExamDto.Language = lang;
            if (_addLevelCombo.SelectedItem is LanguageLevel level) _newExamDto.Level = level;

            _newExamDto.MaxAttendees = int.TryParse(_addMaxBox.Text?.Trim(), out var max) ? max : 0;

            var sr = new CultureInfo("sr-RS");
            if (_addDatePicker.SelectedDate is DateTime d)
                _newExamDto.Date = d.ToString("dd.MM.yyyy", sr);

            _newExamDto.StartAt = _addTimeBox.Text?.Trim() ?? string.Empty;
            _newExamDto.RoomId = string.IsNullOrWhiteSpace(_addRoomBox.Text) ? string.Empty : _addRoomBox.Text.Trim();

            if (!_newExamDto.IsValid)
            {
                MessageBox.Show("Please fix validation errors (date, time, attendees...).",
                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!_newExamDto.TryGetStart(out var start))
            {
                MessageBox.Show("Invalid date/time.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (start <= DateTime.Now)
            {
                MessageBox.Show("Exam start must be in the future.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var term = _newExamDto.ToExamTerm();
                _examController.Create(term);
                _view.ReloadAll();
                CloseAddExamOverlay();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Cannot create exam", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void OpenEditExamOverlay(Window hostWindow, ExamTerm exam)
        {
            _editingExam = exam;

            if (_editLangCombo.Items.Count == 0)
                _editLangCombo.ItemsSource = Enum.GetValues(typeof(Language));
            if (_editLevelCombo.Items.Count == 0)
                _editLevelCombo.ItemsSource = Enum.GetValues(typeof(LanguageLevel));

            _editDatePicker.Language = hostWindow.Language;
            _editDatePicker.SelectedDateFormat = DatePickerFormat.Short;

            _editExamDto = new ExamTermDTO(exam);

            _editLangCombo.SelectedItem = _editExamDto.Language;
            _editLevelCombo.SelectedItem = _editExamDto.Level;
            _editMaxBox.Text = _editExamDto.MaxAttendees.ToString();
            _editRoomBox.Text = _editExamDto.RoomId ?? "";

            var sr = new CultureInfo("sr-RS");
            if (DateTime.TryParseExact(_editExamDto.Date, "dd.MM.yyyy", sr, DateTimeStyles.None, out var d))
                _editDatePicker.SelectedDate = d;
            else
                _editDatePicker.SelectedDate = null;

            _editTimeBox.Text = string.IsNullOrWhiteSpace(_editExamDto.StartAt) ? "10:00" : _editExamDto.StartAt;

            _editOverlay.Visibility = Visibility.Visible;
        }

        public void CloseEditExamOverlay() => _editOverlay.Visibility = Visibility.Collapsed;

        public void EditExam_Click(Window hostWindow, object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not ExamTerm exam) return;

            ExamTermDTO examTermDTO = new ExamTermDTO(exam);
            if (!ProfessorViewExams.TryGetExamStart(examTermDTO, out var start))
            {
                MessageBox.Show("This exam has invalid date/time and cannot be edited.",
                                "Edit not allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if ((start - DateTime.Now).TotalDays < 14)
            {
                var sr = new CultureInfo("sr-RS");
                MessageBox.Show(
                    $"You can edit an exam no later than 14 days before it starts.\n" +
                    $"Exam start: {start.ToString("dd.MM.yyyy HH:mm", sr)}",
                    "Edit not allowed",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            OpenEditExamOverlay(hostWindow, exam);
        }

        public void SaveEditedExam()
        {
            if (_editingExam is null || _editExamDto is null)
            {
                MessageBox.Show("No exam selected.", "Edit", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // UI -> DTO
            if (_editLangCombo.SelectedItem is Language lang) _editExamDto.Language = lang;
            if (_editLevelCombo.SelectedItem is LanguageLevel level) _editExamDto.Level = level;

            _editExamDto.MaxAttendees = int.TryParse(_editMaxBox.Text?.Trim(), out var max) ? max : 0;

            var sr = new CultureInfo("sr-RS");
            if (_editDatePicker.SelectedDate is DateTime d)
                _editExamDto.Date = d.ToString("dd.MM.yyyy", sr);

            _editExamDto.StartAt = _editTimeBox.Text?.Trim() ?? string.Empty;
            _editExamDto.RoomId = string.IsNullOrWhiteSpace(_editRoomBox.Text) ? string.Empty : _editRoomBox.Text.Trim();

            if (!_editExamDto.IsValid)
            {
                MessageBox.Show("Please fix validation errors (date, time, attendees...).",
                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!_editExamDto.TryGetStart(out var start))
            {
                MessageBox.Show("Invalid date/time.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if ((start - DateTime.Now).TotalDays < 14)
            {
                MessageBox.Show(
                    $"You can edit an exam no later than 14 days before it starts.\nExam start: {start:dd.MM.yyyy HH:mm}",
                    "Edit not allowed", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (start <= DateTime.Now)
            {
                MessageBox.Show("Exam start must be in the future.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var updated = _editExamDto.ToExamTerm();
                updated.Id = _editingExam.Id;
                updated.ProfessorId = _professorId;

                var saved = _examController.Update(updated);
                if (saved is null) throw new InvalidOperationException("Update failed.");

                _view.ReloadAll();
                CloseEditExamOverlay();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Cannot update exam", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void CancelExam(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not ExamTerm exam) return;

            var ok = MessageBox.Show($"Cancel exam ID={exam.Id}?", "Confirm",
                                     MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (ok != MessageBoxResult.Yes) return;

            try
            {
                _examController.Delete(exam.Id);
                _view.ReloadAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Cannot cancel", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
