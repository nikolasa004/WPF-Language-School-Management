using LangLang.Controller.TeachingActivities;
using LangLang.DTO;
using LangLang.Model;
using LangLang.Model.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LangLang.RoleWindows.ProfessorViewModules
{
    internal class ProfessorManageCourses
    {
        private readonly CourseController _courseController;
        private readonly string _professorId;

        private readonly List<CourseDTO> _allCourses;
        private readonly ProfessorViewCourses _viewCourses;

        



        private CourseDTO? _editingCourse = null;

        // UI Controls
        private readonly Grid AddCourseOverlay;
        private readonly ComboBox AddCourseLangCombo;
        private readonly ComboBox AddCourseLevelCombo;
        private readonly TextBox AddCourseMaxBox;
        private readonly TextBox AddCourseRoomBox;
        private readonly DatePicker AddCourseDatePicker;
        private readonly TextBox AddCourseTimeBox;
        private readonly CheckBox AddCourseIsLiveCheckBox;
        private readonly CheckBox AddCourseMonCheckBox;
        private readonly CheckBox AddCourseTueCheckBox;
        private readonly CheckBox AddCourseWedCheckBox;
        private readonly CheckBox AddCourseThuCheckBox;
        private readonly CheckBox AddCourseFriCheckBox;
        private readonly CheckBox AddCourseSatCheckBox;
        private readonly CheckBox AddCourseSunCheckBox;
        private readonly TextBox AddCourseDurationWeeksBox;

        private readonly Grid EditCourseOverlay;
        private readonly ComboBox EditCourseLangCombo;
        private readonly ComboBox EditCourseLevelCombo;
        private readonly TextBox EditCourseMaxBox;
        private readonly TextBox EditCourseRoomBox;
        private readonly DatePicker EditCourseDatePicker;
        private readonly TextBox EditCourseTimeBox;
        private readonly CheckBox EditCourseIsLiveCheckBox;
        private readonly CheckBox EditCourseMonCheckBox;
        private readonly CheckBox EditCourseTueCheckBox;
        private readonly CheckBox EditCourseWedCheckBox;
        private readonly CheckBox EditCourseThuCheckBox;
        private readonly CheckBox EditCourseFriCheckBox;
        private readonly CheckBox EditCourseSatCheckBox;
        private readonly CheckBox EditCourseSunCheckBox;
        private readonly TextBox EditCourseDurationWeeksBox;

        public ProfessorManageCourses(
            string professorId,
            List<CourseDTO> allCourses,
            ProfessorViewCourses viewCourses,

            Grid addCourseOverlay,
            ComboBox addLangCombo, ComboBox addLevelCombo,
            TextBox addMaxBox, TextBox addRoomBox,
            DatePicker addDatePicker, TextBox addTimeBox,
            CheckBox addIsLive,
            CheckBox addMon, CheckBox addTue, CheckBox addWed,
            CheckBox addThu, CheckBox addFri, CheckBox addSat, CheckBox addSun,
            TextBox addDurationWeeks,

            Grid editCourseOverlay,
            ComboBox editLangCombo, ComboBox editLevelCombo,
            TextBox editMaxBox, TextBox editRoomBox,
            DatePicker editDatePicker, TextBox editTimeBox,
            CheckBox editIsLive,
            CheckBox editMon, CheckBox editTue, CheckBox editWed,
            CheckBox editThu, CheckBox editFri, CheckBox editSat, CheckBox editSun,
            TextBox editDurationWeeks
        )
        {
            _courseController = new CourseController();
            _professorId = professorId;
            _allCourses = allCourses;
            _viewCourses = viewCourses;

           


        // Assign controls
        AddCourseOverlay = addCourseOverlay;
            AddCourseLangCombo = addLangCombo;
            AddCourseLevelCombo = addLevelCombo;
            AddCourseMaxBox = addMaxBox;
            AddCourseRoomBox = addRoomBox;
            AddCourseDatePicker = addDatePicker;
            AddCourseTimeBox = addTimeBox;
            AddCourseIsLiveCheckBox = addIsLive;
            AddCourseMonCheckBox = addMon;
            AddCourseTueCheckBox = addTue;
            AddCourseWedCheckBox = addWed;
            AddCourseThuCheckBox = addThu;
            AddCourseFriCheckBox = addFri;
            AddCourseSatCheckBox = addSat;
            AddCourseSunCheckBox = addSun;
            AddCourseDurationWeeksBox = addDurationWeeks;

            EditCourseOverlay = editCourseOverlay;
            EditCourseLangCombo = editLangCombo;
            EditCourseLevelCombo = editLevelCombo;
            EditCourseMaxBox = editMaxBox;
            EditCourseRoomBox = editRoomBox;
            EditCourseDatePicker = editDatePicker;
            EditCourseTimeBox = editTimeBox;
            EditCourseIsLiveCheckBox = editIsLive;
            EditCourseMonCheckBox = editMon;
            EditCourseTueCheckBox = editTue;
            EditCourseWedCheckBox = editWed;
            EditCourseThuCheckBox = editThu;
            EditCourseFriCheckBox = editFri;
            EditCourseSatCheckBox = editSat;
            EditCourseSunCheckBox = editSun;
            EditCourseDurationWeeksBox = editDurationWeeks;

            InitComboBoxes();
        }

        private void InitComboBoxes()
        {
            if (AddCourseLangCombo.Items.Count == 0) AddCourseLangCombo.ItemsSource = Enum.GetValues(typeof(Language));
            if (AddCourseLevelCombo.Items.Count == 0) AddCourseLevelCombo.ItemsSource = Enum.GetValues(typeof(LanguageLevel));
            if (EditCourseLangCombo.Items.Count == 0) EditCourseLangCombo.ItemsSource = Enum.GetValues(typeof(Language));
            if (EditCourseLevelCombo.Items.Count == 0) EditCourseLevelCombo.ItemsSource = Enum.GetValues(typeof(LanguageLevel));
        }

        // ===== Overlay methods =====
        public void OpenAddCourseOverlay()
        {
            AddCourseLangCombo.SelectedItem = null;
            AddCourseLevelCombo.SelectedItem = null;
            AddCourseMaxBox.Text = "";
            AddCourseRoomBox.Text = "";
            AddCourseDatePicker.SelectedDate = null;
            AddCourseTimeBox.Text = "10:00";
            AddCourseIsLiveCheckBox.IsChecked = false;
            AddCourseMonCheckBox.IsChecked = false;
            AddCourseTueCheckBox.IsChecked = false;
            AddCourseWedCheckBox.IsChecked = false;
            AddCourseThuCheckBox.IsChecked = false;
            AddCourseFriCheckBox.IsChecked = false;
            AddCourseSatCheckBox.IsChecked = false;
            AddCourseSunCheckBox.IsChecked = false;
            AddCourseDurationWeeksBox.Text = "";

            AddCourseOverlay.Visibility = Visibility.Visible;
        }

        public void CloseAddCourseOverlay() => AddCourseOverlay.Visibility = Visibility.Collapsed;

        public void OpenEditCourseOverlay(CourseDTO course)
        {
            _editingCourse = course;

            EditCourseLangCombo.SelectedItem = course.Language;
            EditCourseLevelCombo.SelectedItem = course.Level;
            EditCourseMaxBox.Text = course.MaxAttendees?.ToString() ?? "";
            EditCourseRoomBox.Text = course.RoomId ?? "";
            EditCourseDatePicker.SelectedDate = DateTime.TryParse(course.Date, out var d) ? d : null;
            EditCourseTimeBox.Text = string.IsNullOrWhiteSpace(course.StartAt) ? "10:00" : course.StartAt;
            EditCourseIsLiveCheckBox.IsChecked = course.IsLive;
            EditCourseMonCheckBox.IsChecked = course.Days.Contains(DayOfWeek.Monday);
            EditCourseTueCheckBox.IsChecked = course.Days.Contains(DayOfWeek.Tuesday);
            EditCourseWedCheckBox.IsChecked = course.Days.Contains(DayOfWeek.Wednesday);
            EditCourseThuCheckBox.IsChecked = course.Days.Contains(DayOfWeek.Thursday);
            EditCourseFriCheckBox.IsChecked = course.Days.Contains(DayOfWeek.Friday);
            EditCourseSatCheckBox.IsChecked = course.Days.Contains(DayOfWeek.Saturday);
            EditCourseSunCheckBox.IsChecked = course.Days.Contains(DayOfWeek.Sunday);
            EditCourseDurationWeeksBox.Text = course.DurationWeeks.ToString();

            EditCourseOverlay.Visibility = Visibility.Visible;
        }

        public void CloseEditCourseOverlay()
        {
            _editingCourse = null;
            EditCourseOverlay.Visibility = Visibility.Collapsed;
        }

        // ===== CRUD methods =====
        public void SaveNewCourse()
        {
            if (AddCourseLangCombo.SelectedItem is not Language lang)
            {
                MessageBox.Show("Please select language.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (AddCourseLevelCombo.SelectedItem is not LanguageLevel level)
            {
                MessageBox.Show("Please select level.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool isLive = AddCourseIsLiveCheckBox.IsChecked == true;

            int max = 0;
            if (isLive)
            {
                if (!int.TryParse(AddCourseMaxBox.Text?.Trim(), out max) || max <= 0)
                {
                    MessageBox.Show("Max attendees must be a number greater than 0 for in-person courses.",
                                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            if (!AddCourseDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Please choose course start date.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime selectedDate = AddCourseDatePicker.SelectedDate.Value;

            if (string.IsNullOrWhiteSpace(AddCourseTimeBox.Text) ||
                !TimeSpan.TryParseExact(AddCourseTimeBox.Text.Trim(), @"hh\:mm", CultureInfo.InvariantCulture, out var time))
            {
                MessageBox.Show("Time must be in HH:mm (e.g. 10:00).", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var days = new List<DayOfWeek>();
            if (AddCourseMonCheckBox.IsChecked == true) days.Add(DayOfWeek.Monday);
            if (AddCourseTueCheckBox.IsChecked == true) days.Add(DayOfWeek.Tuesday);
            if (AddCourseWedCheckBox.IsChecked == true) days.Add(DayOfWeek.Wednesday);
            if (AddCourseThuCheckBox.IsChecked == true) days.Add(DayOfWeek.Thursday);
            if (AddCourseFriCheckBox.IsChecked == true) days.Add(DayOfWeek.Friday);
            if (AddCourseSatCheckBox.IsChecked == true) days.Add(DayOfWeek.Saturday);
            if (AddCourseSunCheckBox.IsChecked == true) days.Add(DayOfWeek.Sunday);

            if (days.Count == 0)
            {
                MessageBox.Show("Please select at least one day for the course.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(AddCourseDurationWeeksBox.Text?.Trim(), out int durationWeeks) || durationWeeks <= 0)
            {
                MessageBox.Show("Duration (weeks) must be a number greater than 0.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newCourse = new CourseDTO
            {
                Id = 0,
                ProfessorId = _professorId,
                Language = lang,
                Level = level,
                MaxAttendees = max,
                Date = selectedDate.ToString("dd.MM.yyyy"),
                StartAt = AddCourseTimeBox.Text.Trim(),
                RoomId = string.IsNullOrWhiteSpace(AddCourseRoomBox.Text) ? null : AddCourseRoomBox.Text.Trim(),
                IsLive = isLive,
                Days = days,
                DurationWeeks = durationWeeks
            };

            try
            {
                var saved = _courseController.Create(newCourse.ToCourse());
                _allCourses.Add(new CourseDTO(saved));
                _viewCourses.ApplyFiltersSortAndPageForCourses(resetPage: false);
                CloseAddCourseOverlay();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Cannot create course", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void SaveEditedCourse()
        {
            if (_editingCourse is null) return;

            if (EditCourseLangCombo.SelectedItem is not Language lang)
            {
                MessageBox.Show("Please select language.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (EditCourseLevelCombo.SelectedItem is not LanguageLevel level)
            {
                MessageBox.Show("Please select level.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool isLive = EditCourseIsLiveCheckBox.IsChecked == true;

            int max = 0;
            if (isLive)
            {
                if (!int.TryParse(EditCourseMaxBox.Text?.Trim(), out max) || max <= 0)
                {
                    MessageBox.Show("Max attendees must be a number greater than 0 for in-person courses.",
                                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            if (!EditCourseDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Please choose course start date.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime selectedDate = EditCourseDatePicker.SelectedDate.Value;

            if (string.IsNullOrWhiteSpace(EditCourseTimeBox.Text) ||
                !TimeSpan.TryParseExact(EditCourseTimeBox.Text.Trim(), @"hh\:mm", CultureInfo.InvariantCulture, out var time))
            {
                MessageBox.Show("Time must be in HH:mm (e.g. 10:00).", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var days = new List<DayOfWeek>();
            if (EditCourseMonCheckBox.IsChecked == true) days.Add(DayOfWeek.Monday);
            if (EditCourseTueCheckBox.IsChecked == true) days.Add(DayOfWeek.Tuesday);
            if (EditCourseWedCheckBox.IsChecked == true) days.Add(DayOfWeek.Wednesday);
            if (EditCourseThuCheckBox.IsChecked == true) days.Add(DayOfWeek.Thursday);
            if (EditCourseFriCheckBox.IsChecked == true) days.Add(DayOfWeek.Friday);
            if (EditCourseSatCheckBox.IsChecked == true) days.Add(DayOfWeek.Saturday);
            if (EditCourseSunCheckBox.IsChecked == true) days.Add(DayOfWeek.Sunday);

            if (days.Count == 0)
            {
                MessageBox.Show("Please select at least one day for the course.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(EditCourseDurationWeeksBox.Text?.Trim(), out int durationWeeks) || durationWeeks <= 0)
            {
                MessageBox.Show("Duration (weeks) must be a number greater than 0.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            
            _editingCourse.Language = lang;
            _editingCourse.Level = level;
            _editingCourse.MaxAttendees = max;
            _editingCourse.Date = selectedDate.ToString("dd.MM.yyyy");
            _editingCourse.StartAt = EditCourseTimeBox.Text.Trim();
            _editingCourse.RoomId = string.IsNullOrWhiteSpace(EditCourseRoomBox.Text) ? null : EditCourseRoomBox.Text.Trim();
            _editingCourse.IsLive = isLive;
            _editingCourse.Days = days;
            _editingCourse.DurationWeeks = durationWeeks;

            try
            {
                var saved = _courseController.Update(_editingCourse.ToCourse());
                if (saved is null) throw new InvalidOperationException("Update failed.");

                var idx = _allCourses.FindIndex(x => x.Id == saved.Id);
                if (idx >= 0) _allCourses[idx] = new CourseDTO(saved);

                _viewCourses.ApplyFiltersSortAndPageForCourses(resetPage: false);
                CloseEditCourseOverlay();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Cannot update course", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void DeleteCourse(CourseDTO course)
        {
            var ok = MessageBox.Show($"Delete course ID={course.Id}?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (ok != MessageBoxResult.Yes) return;

            try
            {
                _courseController.Delete(course.Id);
                _allCourses.RemoveAll(x => x.Id == course.Id);
                _viewCourses.ApplyFiltersSortAndPageForCourses(resetPage: false);
              
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Cannot delete", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


    }
}

