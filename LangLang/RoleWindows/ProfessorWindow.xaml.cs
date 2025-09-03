using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using LangLang.Controller;
using LangLang.DTO;
using LangLang.Model;
using LangLang.Model.Enums;
using LangLang.RoleWindows.StudentViewModules;

namespace LangLang.RoleWindows
{
    public partial class ProfessorWindow : Window
    {
        // ===== Public view models (unchanged) =====
        public ProfessorDTO Professor { get; set; }
        public ProfessorDTO UpdateProfessor { get; set; }

        private CourseDTO? _editingCourse = null;

        // ===== Controllers / state =====
        private readonly ProfessorsController professorsController;
        private readonly ExamTermsController _examController = new ExamTermsController();
        private readonly string _professorId;

        // Add/Edit overlay helper
        private ExamTerm? _editingExam;

        //Course

        private readonly CourseController _courseController = new();

        private List<CourseDTO> _allCourses = new();
        private List<CourseDTO> _filteredCourses = new();   // after filters
        private List<CourseDTO> _sortedCourses = new();     // after sorting
        private readonly ObservableCollection<CourseDTO> _pageOfCourses = new();

        private int _pageSizeForCourses = 10;
        private int _currentPageForCourses = 1;

        // ===== Sorting & pagination pipeline =====
        private List<ExamTerm> _allExams = new();   // all from DAO for this professor
        private List<ExamTerm> _filtered = new();   // after filters
        private List<ExamTerm> _sorted = new();     // after sorting
        private readonly ObservableCollection<ExamTerm> _page = new(); // currently shown page

        private int _pageSize = 10;
        private int _currentPage = 1;
        private string _sortKey = "Date";
        private ListSortDirection _sortDir = ListSortDirection.Ascending;

        // ===== ctor =====
        public ProfessorWindow(Professor professor,
                        ProfessorsController professorsController)
        {
            InitializeComponent();

            // sr-RS culture only for this window
            var sr = new CultureInfo("sr-RS");
            Thread.CurrentThread.CurrentCulture = sr;
            Thread.CurrentThread.CurrentUICulture = sr;
            this.Language = XmlLanguage.GetLanguage(sr.IetfLanguageTag);

            this.Loaded += (_, __) =>
            {
                if (ExamFromDate != null)
                {
                    ExamFromDate.Language = this.Language;
                    ExamFromDate.SelectedDateFormat = DatePickerFormat.Short;
                }
                if (ExamToDate != null)
                {
                    ExamToDate.Language = this.Language;
                    ExamToDate.SelectedDateFormat = DatePickerFormat.Short;
                }

                InitAddExamOverlay();

            };

            DataContext = this;

            Professor = new ProfessorDTO(professor);
            UpdateProfessor = new ProfessorDTO(professor);

            this.professorsController = professorsController;

            _professorId = Professor.Email;

            ExamsGrid.ItemsSource = _page;
            _allExams = _examController.GetMyExamTerms(_professorId).ToList();
            ApplyFiltersSortAndPage();

            CoursesGrid.ItemsSource = _pageOfCourses;
            _allCourses = _courseController.GetMyCourses(_professorId)
                                .Select(c => new CourseDTO(c))
                                .ToList();
            ApplyFiltersSortAndPageForCourses();

            InformationUpdate();
        }

        private void InformationUpdate()
        {
            NameLabel.Content = "Name: " + Professor.FirstName;
            SurnameLabel.Content = "Surname: " + Professor.LastName;
            GenderLabel.Content = "Gender: " + Professor.Gender;

            if (DateTime.TryParseExact(Professor.DateOfBirth, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime birthDate))
                BirthDateLabel.Content = "Birth Date: " + birthDate.ToString("dd.MM.yyyy", new CultureInfo("sr-RS"));
            else
                BirthDateLabel.Content = "Birth Date: " + Professor.DateOfBirth;

            PhoneLabel.Content = "Phone: " + Professor.PhoneNumber;
            EmailLabel.Content = "Email: " + Professor.Email;
            EducationLabel.Content = "Education: " + Professor.EducationLevel.ToString();

            if (Professor.Languages != null && Professor.Languages.Count > 0)
            {
                var languagesText = string.Join(", ", Professor.Languages.Select(kvp => $"{kvp.Key} - {kvp.Value}"));
                LanguagesLabel.Text = "Languages: " + languagesText;
            }
            else
            {
                LanguagesLabel.Text = "Languages: -";
            }

            if (DateTime.TryParseExact(Professor.DateOfRegistration, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime joinedDate))
                JoinedDateLabel.Content = "Joined Date: " + joinedDate.ToString("dd.MM.yyyy", new CultureInfo("sr-RS"));
            else
                JoinedDateLabel.Content = "Joined Date: " + Professor.DateOfRegistration;
        }

        private void InitAddExamOverlay()
        {
            if (AddExamLangCombo != null && AddExamLangCombo.Items.Count == 0)
                AddExamLangCombo.ItemsSource = Enum.GetValues(typeof(Language));

            if (AddExamLevelCombo != null && AddExamLevelCombo.Items.Count == 0)
                AddExamLevelCombo.ItemsSource = Enum.GetValues(typeof(LanguageLevel));

            if (AddExamDatePicker != null)
            {
                AddExamDatePicker.Language = this.Language;
                AddExamDatePicker.SelectedDateFormat = DatePickerFormat.Short;
                AddExamDatePicker.SelectedDate = DateTime.Now.Date.AddDays(1);
            }

            if (AddExamTimeBox != null)
                AddExamTimeBox.Text = "10:00";

            if (AddExamMaxBox != null) AddExamMaxBox.Text = "10";
            if (AddExamRoomBox != null) AddExamRoomBox.Text = "";
        }

        private void OpenAddExamOverlay()
        {
            InitAddExamOverlay();
            AddExamOverlay.Visibility = Visibility.Visible;
        }

        private void CloseAddExamOverlay()
        {
            AddExamOverlay.Visibility = Visibility.Collapsed;
        }

        private void AddExam_Click(object sender, RoutedEventArgs e) => OpenAddExamOverlay();

        private void SaveNewExam_Click(object sender, RoutedEventArgs e)
        {
            if (AddExamLangCombo.SelectedItem is not Language lang)
            {
                MessageBox.Show("Please select language.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (AddExamLevelCombo.SelectedItem is not LanguageLevel level)
            {
                MessageBox.Show("Please select level.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!int.TryParse(AddExamMaxBox.Text?.Trim(), out int max) || max <= 0)
            {
                MessageBox.Show("Max attendees must be a number greater than 0.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (AddExamDatePicker.SelectedDate is not DateTime date)
            {
                MessageBox.Show("Please choose exam date.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(AddExamTimeBox.Text) ||
                !TimeSpan.TryParseExact(AddExamTimeBox.Text.Trim(), @"hh\:mm", CultureInfo.InvariantCulture, out var time))
            {
                MessageBox.Show("Time must be in HH:mm (e.g. 10:00).", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var start = date.Date.Add(time);
            if (start <= DateTime.Now)
            {
                MessageBox.Show("Exam start must be in the future.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sr = new CultureInfo("sr-RS");
            var term = new ExamTerm
            {
                ProfessorId = _professorId,
                Language = lang,
                Level = level,
                IsLive = true,
                MaxAttendees = max,
                Date = start.ToString("dd.MM.yyyy", sr),
                StartAt = start.ToString("HH:mm", CultureInfo.InvariantCulture),
                Duration = 240,
                RoomId = string.IsNullOrWhiteSpace(AddExamRoomBox.Text) ? null : AddExamRoomBox.Text.Trim()
            };

            try
            {
                var created = _examController.Create(term);
                _allExams.Add(created);
                ApplyFiltersSortAndPage(resetPage: false);
                CloseAddExamOverlay();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Cannot create exam", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelNewExam_Click(object sender, RoutedEventArgs e) => CloseAddExamOverlay();

        private void OpenEditExamOverlay(ExamTerm exam)
        {
            _editingExam = exam;

            if (EditExamLangCombo.Items.Count == 0)
                EditExamLangCombo.ItemsSource = Enum.GetValues(typeof(Language));
            if (EditExamLevelCombo.Items.Count == 0)
                EditExamLevelCombo.ItemsSource = Enum.GetValues(typeof(LanguageLevel));

            EditExamDatePicker.Language = this.Language;
            EditExamDatePicker.SelectedDateFormat = DatePickerFormat.Short;

            EditExamLangCombo.SelectedItem = exam.Language;
            EditExamLevelCombo.SelectedItem = exam.Level;
            EditExamMaxBox.Text = (exam.MaxAttendees ?? 0).ToString();
            EditExamRoomBox.Text = exam.RoomId ?? "";

            if (TryParseExamDateString(exam.Date, out var d)) EditExamDatePicker.SelectedDate = d.Date;
            else EditExamDatePicker.SelectedDate = null;

            EditExamTimeBox.Text = string.IsNullOrWhiteSpace(exam.StartAt) ? "10:00" : exam.StartAt;

            EditExamOverlay.Visibility = Visibility.Visible;
        }

        private void CloseEditExamOverlay()
        {
            EditExamOverlay.Visibility = Visibility.Collapsed;
        }

        private void EditExam_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not ExamTerm exam) return;

            if (!TryGetExamStart(exam, out var start))
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

            OpenEditExamOverlay(exam);
        }

        private void SaveEditedExam_Click(object sender, RoutedEventArgs e)
        {
            if (_editingExam is null)
            {
                MessageBox.Show("No exam selected.", "Edit", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (EditExamLangCombo.SelectedItem is not Language lang)
            {
                MessageBox.Show("Please select language.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (EditExamLevelCombo.SelectedItem is not LanguageLevel level)
            {
                MessageBox.Show("Please select level.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!int.TryParse(EditExamMaxBox.Text?.Trim(), out int max) || max <= 0)
            {
                MessageBox.Show("Max attendees must be a number greater than 0.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (EditExamDatePicker.SelectedDate is not DateTime date)
            {
                MessageBox.Show("Please choose exam date.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(EditExamTimeBox.Text) ||
                !TimeSpan.TryParseExact(EditExamTimeBox.Text.Trim(), @"hh\:mm", CultureInfo.InvariantCulture, out var time))
            {
                MessageBox.Show("Time must be in HH:mm (e.g. 10:00).", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var start = date.Date.Add(time);
            if (start <= DateTime.Now)
            {
                MessageBox.Show("Exam start must be in the future.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sr = new CultureInfo("sr-RS");
            var updated = new ExamTerm
            {
                Id = _editingExam.Id,
                ProfessorId = _professorId,
                Language = lang,
                Level = level,
                IsLive = true,
                MaxAttendees = max,
                Date = start.ToString("dd.MM.yyyy", sr),
                StartAt = start.ToString("HH:mm", CultureInfo.InvariantCulture),
                Duration = 240,
                RoomId = string.IsNullOrWhiteSpace(EditExamRoomBox.Text) ? null : EditExamRoomBox.Text.Trim()
            };

            try
            {
                var saved = _examController.Update(updated);
                if (saved is null) throw new InvalidOperationException("Update failed.");

                var idx = _allExams.FindIndex(x => x.Id == saved.Id);
                if (idx >= 0) _allExams[idx] = saved;
                ApplyFiltersSortAndPage(resetPage: false);

                CloseEditExamOverlay();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Cannot update exam", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelEditExam_Click(object sender, RoutedEventArgs e) => CloseEditExamOverlay();

        private void InitExamFilters()
        {
            if (ExamLanguageCombo.Items.Count == 0)
                ExamLanguageCombo.ItemsSource = Enum.GetValues(typeof(Language));

            if (ExamLevelCombo.Items.Count == 0)
                ExamLevelCombo.ItemsSource = Enum.GetValues(typeof(LanguageLevel));

            ExamLanguageCombo.SelectedIndex = -1;
            ExamLevelCombo.SelectedIndex = -1;

            if (ExamFromDate != null)
            {
                ExamFromDate.Language = this.Language;
                ExamFromDate.SelectedDateFormat = DatePickerFormat.Short;
                ExamFromDate.SelectedDate = null;
            }
            if (ExamToDate != null)
            {
                ExamToDate.Language = this.Language;
                ExamToDate.SelectedDateFormat = DatePickerFormat.Short;
                ExamToDate.SelectedDate = null;
            }
        }

        private void FindYourExams_Click(object sender, RoutedEventArgs e)
        {

            ApplyFiltersSortAndPage(resetPage: true);
        }

        private List<ExamTerm> ApplyCurrentFilters(List<ExamTerm> input)
        {
            Language? lang = null;
            LanguageLevel? level = null;

            if (ExamLanguageCombo.SelectedItem is Language selLang) lang = selLang;
            if (ExamLevelCombo.SelectedItem is LanguageLevel selLevel) level = selLevel;

            DateTime? from = ExamFromDate.SelectedDate?.Date;
            DateTime? to = ExamToDate.SelectedDate?.Date;

            if (from.HasValue && to.HasValue && from.Value > to.Value)
                (from, to) = (to, from);


            return _examController.SearchMyExamTerms(_professorId, lang, level, from, to);
        }

        private void ExamsButton_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Visibility = Visibility.Collapsed;
            AddCourseOverlay.Visibility = Visibility.Collapsed;
            ExamsPanel.Visibility = Visibility.Visible;

            InitExamFilters();


            _allExams = _examController.GetMyExamTerms(_professorId).ToList();
            ApplyFiltersSortAndPage();
        }

        private void CloseExamsPanel_Click(object sender, RoutedEventArgs e)
        {
            ExamsPanel.Visibility = Visibility.Collapsed;
            AddCourseOverlay.Visibility = Visibility.Collapsed;
            MainPanel.Visibility = Visibility.Visible;
        }

        private void CancelExam_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not ExamTerm exam) return;

            var ok = MessageBox.Show($"Cancel exam ID={exam.Id}?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (ok != MessageBoxResult.Yes) return;

            try
            {
                _examController.Delete(exam.Id);
                _allExams.RemoveAll(x => x.Id == exam.Id);
                ApplyFiltersSortAndPage(resetPage: false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Cannot cancel", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ApplyFiltersSortAndPage(bool resetPage = true)
        {
            // 1) filter
            _filtered = ApplyCurrentFilters(_allExams);

            // 2) sort
            _sorted = ApplySort(_filtered, _sortKey, _sortDir).ToList();

            // 3) page
            var totalPages = Math.Max(1, (int)Math.Ceiling((double)_sorted.Count / _pageSize));
            if (resetPage) _currentPage = 1;
            _currentPage = Math.Min(Math.Max(_currentPage, 1), totalPages);

            var slice = _sorted
                .Skip((_currentPage - 1) * _pageSize)
                .Take(_pageSize)
                .ToList();

            _page.Clear();
            foreach (var x in slice) _page.Add(x);


            if (FindName("PagerText") is TextBlock pager)
                pager.Text = $"Page {_currentPage}/{totalPages}  ·  {(_sorted.Count == 1 ? "1 item" : _sorted.Count + " items")}";
        }

        private IEnumerable<ExamTerm> ApplySort(IEnumerable<ExamTerm> source, string sortKey, ListSortDirection dir)
        {

            var key = NormalizeSortKey(sortKey);

            Func<ExamTerm, object> keySelector = key switch
            {
                "Id" => e => e.Id,
                "Language" => e => e.Language.ToString(),
                "Level" => e => (int)e.Level,
                "RoomId" => e => e.RoomId ?? "",
                "MaxAttendees" => e => e.MaxAttendees ?? 0,
                "StartAt" => e => GetStartDateTime(e) ?? DateTime.MinValue,
                "Date" => e => GetStartDateTime(e) ?? DateTime.MinValue,
                _ => e => GetStartDateTime(e) ?? DateTime.MinValue
            };

            return dir == ListSortDirection.Ascending
                ? source.OrderBy(keySelector)
                : source.OrderByDescending(keySelector);
        }

        private static string NormalizeSortKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return "Date";
            key = key.Trim();


            return key.ToLower() switch
            {
                "id" => "Id",
                "language" => "Language",
                "level" => "Level",
                "date" => "Date",
                "start" => "StartAt",
                "room" => "RoomId",
                "max" => "MaxAttendees",
                _ => key
            };
        }

        private static DateTime? GetStartDateTime(ExamTerm e)
        {
            if (!DateTime.TryParseExact(e.Date ?? "", "dd.MM.yyyy", new CultureInfo("sr-RS"),
                                        DateTimeStyles.None, out var d) &&
                !DateTime.TryParseExact(e.Date ?? "", "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                        DateTimeStyles.None, out d))
                return null;

            if (!TimeSpan.TryParseExact(e.StartAt ?? "", @"hh\:mm", CultureInfo.InvariantCulture, out var ts))
                return null;

            return d.Date.Add(ts);
        }


        private void ExamsGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;


            foreach (var c in ExamsGrid.Columns) c.SortDirection = null;

            var key = e.Column.SortMemberPath ?? e.Column.Header?.ToString() ?? "Date";
            key = NormalizeSortKey(key);

            if (_sortKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                _sortDir = _sortDir == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            else
            {
                _sortKey = key;
                _sortDir = ListSortDirection.Ascending;
            }

            e.Column.SortDirection = _sortDir;
            ApplyFiltersSortAndPage(resetPage: false);
        }

        private void PrevPage_Click(object? sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                ApplyFiltersSortAndPage(resetPage: false);
            }
        }

        private void NextPage_Click(object? sender, RoutedEventArgs e)
        {
            var totalPages = Math.Max(1, (int)Math.Ceiling((double)_sorted.Count / _pageSize));
            if (_currentPage < totalPages)
            {
                _currentPage++;
                ApplyFiltersSortAndPage(resetPage: false);
            }
        }

        private void PageSizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            if (sender is ComboBox cb &&
                cb.SelectedItem is ComboBoxItem it &&
                int.TryParse(it.Content?.ToString(), out var ps) &&
                ps > 0)
            {
                _pageSize = ps;
                _currentPage = 1;
                ApplyFiltersSortAndPage(resetPage: true);
            }
        }

        private static bool TryParseExamDateString(string? s, out DateTime date)
        {
            date = default;
            if (string.IsNullOrWhiteSpace(s)) return false;

            if (DateTime.TryParseExact(s, "dd.MM.yyyy", new CultureInfo("sr-RS"),
                                       DateTimeStyles.None, out date))
                return true;

            if (DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                       DateTimeStyles.None, out date))
                return true;

            return false;
        }

        private static bool TryGetExamStart(ExamTerm exam, out DateTime start)
        {
            start = default;

            if (!DateTime.TryParseExact(exam.Date ?? "", "dd.MM.yyyy", new CultureInfo("sr-RS"),
                                        DateTimeStyles.None, out var d) &&
                !DateTime.TryParseExact(exam.Date ?? "", "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                        DateTimeStyles.None, out d))
                return false;

            if (!TimeSpan.TryParseExact(exam.StartAt ?? "", @"hh\:mm", CultureInfo.InvariantCulture, out var ts))
                return false;

            start = d.Date.Add(ts);
            return true;
        }

        private void OpenAddCourseOverlay()
        {
            if (AddCourseLangCombo.Items.Count == 0)
                AddCourseLangCombo.ItemsSource = Enum.GetValues(typeof(Language));
            if (AddCourseLevelCombo.Items.Count == 0)
                AddCourseLevelCombo.ItemsSource = Enum.GetValues(typeof(LanguageLevel));

            AddCourseDatePicker.Language = this.Language;
            AddCourseDatePicker.SelectedDateFormat = DatePickerFormat.Short;

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

        private void CloseAddCourseOverlay()
        {
            AddCourseOverlay.Visibility = Visibility.Collapsed;
        }

        private void AddCourseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenAddCourseOverlay();
        }

        private void CancelAddCourse_Click(object sender, RoutedEventArgs e)
        {
            CloseAddCourseOverlay();
        }

        private void SaveNewCourse_Click(object sender, RoutedEventArgs e)
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

                Id = 0000,
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
                _allCourses.Add(newCourse);
                ApplyFiltersSortAndPageForCourses(resetPage: false);
                CloseAddCourseOverlay();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Cannot create exam", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OpenEditCourseOverlay(CourseDTO course)
        {
            _editingCourse = course;

            if (EditCourseLangCombo.Items.Count == 0)
                EditCourseLangCombo.ItemsSource = Enum.GetValues(typeof(Language));
            if (EditCourseLevelCombo.Items.Count == 0)
                EditCourseLevelCombo.ItemsSource = Enum.GetValues(typeof(LanguageLevel));

            EditCourseDatePicker.Language = this.Language;
            EditCourseDatePicker.SelectedDateFormat = DatePickerFormat.Short;

            EditCourseLangCombo.SelectedItem = course.Language;
            EditCourseLevelCombo.SelectedItem = course.Level;
            EditCourseMaxBox.Text = course.MaxAttendees?.ToString() ?? "";
            EditCourseRoomBox.Text = course.RoomId ?? "";

            if (DateTime.TryParse(course.Date, out var d))
                EditCourseDatePicker.SelectedDate = d.Date;
            else
                EditCourseDatePicker.SelectedDate = null;

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

        private void CloseEditCourseOverlay()
        {
            _editingCourse = null;
            EditCourseOverlay.Visibility = Visibility.Collapsed;
        }
        private void EditCourse_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not CourseDTO course) return;

            OpenEditCourseOverlay(course);
        }
        private void SaveEditedCourse_Click(object sender, RoutedEventArgs e)
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
            int max=0;
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

            _editingCourse.Date = selectedDate.ToString("dd.MM.yyyy");



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

                ApplyFiltersSortAndPageForCourses(resetPage: false);

                CloseEditCourseOverlay();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Cannot update course", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }



        private void CancelEditCourse_Click(object sender, RoutedEventArgs e)
        {
            CloseEditCourseOverlay();
        }

        private void DeleteCourse_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not CourseDTO course) return;

            var ok = MessageBox.Show($"Delete course ID={course.Id}?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (ok != MessageBoxResult.Yes) return;

            try
            {
                _courseController.Delete(course.Id);
                _allCourses.RemoveAll(x => x.Id == course.Id);
                ApplyFiltersSortAndPageForCourses(resetPage: false);




            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Cannot delete", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void CoursesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InitCourseFilters();
                CoursesPanel.Visibility = Visibility.Visible;
                MainPanel.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška pri otvaranju Courses panela:\n" + ex.ToString());
            }
        }
        // Course Terms Panel
        private void ShowAllCourses_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Visibility = Visibility.Collapsed;
            CoursesPanel.Visibility = Visibility.Visible;
        }

        private void CloseCoursesPanel_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Visibility = Visibility.Visible;
            CoursesPanel.Visibility = Visibility.Collapsed;
        }

        private void CoursesSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchCourses(sender, e);
        }

        private void PrevPageCourses_Click(object sender, RoutedEventArgs e)
        {
            PrevPageForCourses(sender, e);
        }

        private void NextPageCourses_Click(object sender, RoutedEventArgs e)
        {
            NextPageForCourses(sender, e);
        }

        private void PageSizeComboCourses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PageSizeChanged(sender, e);
        }

        private void CoursesGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            CoursesGridSorting(sender, e);
        }

        private void InitCourseFilters()
        {
            if (CourseLanguageCombo.Items.Count == 0)
                CourseLanguageCombo.ItemsSource = Enum.GetValues(typeof(Language));

            if (CourseLevelCombo.Items.Count == 0)
                CourseLevelCombo.ItemsSource = Enum.GetValues(typeof(LanguageLevel));

            CourseLanguageCombo.SelectedIndex = -1;
            CourseLevelCombo.SelectedIndex = -1;
        }

        private List<CourseDTO> GetAvailableCourses(List<Course> allCourses)
        {
            var now = DateTime.Now;
            List<CourseDTO> available = new();
            foreach (var course in allCourses)
            {
                /////////////////To add
                //exam not full
                CourseDTO courseDto = new CourseDTO(course);
                var start = GetStartDateTime(courseDto);
                if (start.HasValue && start.Value > now.AddDays(7))
                {
                    available.Add(courseDto);
                }
            }

            return available;
        }

        public void SearchCourses(object sender, RoutedEventArgs e)
        {
            ApplyFiltersSortAndPageForCourses(resetPage: true);
        }

        private List<CourseDTO> ApplyCurrentFiltersForCourses(List<CourseDTO> input)
        {
            Language? lang = null;
            LanguageLevel? level = null;

            if (CourseLanguageCombo.SelectedItem is Language selLang) lang = selLang;
            if (CourseLevelCombo.SelectedItem is LanguageLevel selLevel) level = selLevel;

            DateTime? from = CourseFromDate.SelectedDate?.Date;
            DateTime? to = CourseToDate.SelectedDate?.Date;

            if (from.HasValue && to.HasValue && from.Value > to.Value)
                (from, to) = (to, from);

            bool? isLive;
            if (CourseOnlineCombo.SelectedItem is ComboBoxItem comboItem)
            {
                var val = comboItem.Content?.ToString();
                if (val == "Yes")
                    isLive = true;
                else if (val == "No")
                    isLive = false;
                else
                    isLive = null;
            }
            else
                isLive = null;

            int weeks = 0;
            if (int.TryParse(CourseWeeksBox.Text, out var w) && w > 0)
                weeks = w;

            List<Course> inputRaw = input.Select(dto => dto.ToCourse()).ToList();
            List<Course> resultRaw = _courseController.SearchCourses(inputRaw, lang, level, isLive, weeks, from, to);
            List<CourseDTO> result = resultRaw.Select(et => new CourseDTO(et)).ToList();

            return result;
        }

        private IEnumerable<CourseDTO> ApplySortForCourses(IEnumerable<CourseDTO> source, string sortKey, ListSortDirection dir)
        {
            var key = NormalizeSortKeyForCourses(sortKey);

            Func<CourseDTO, object> keySelector = key switch
            {
                "Id" => e => e.Id,
                "Language" => e => e.Language.ToString(),
                "Level" => e => (int)e.Level,
                "ProfessorId" => e => e.ProfessorId ?? "",
                "DurationWeeks" => e => e.DurationWeeks,
                "FormattedDays" => e => e.DaysFormatted ?? "",
                "IsLive" => e => e.IsLive,
                "RoomId" => e => e.RoomId ?? "",
                "MaxAttendees" => e => e.MaxAttendees ?? 0,
                "StartAt" => e => GetStartDateTime(e) ?? DateTime.MinValue,
                "Date" => e => GetStartDateTime(e) ?? DateTime.MinValue,
                _ => e => GetStartDateTime(e) ?? DateTime.MinValue
            };

            return dir == ListSortDirection.Ascending
                ? source.OrderBy(keySelector)
                : source.OrderByDescending(keySelector);
        }

        private static string NormalizeSortKeyForCourses(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return "Date";
            key = key.Trim();


            return key.ToLower() switch
            {
                "id" => "Id",
                "language" => "Language",
                "level" => "Level",
                "professor" => "ProfessorId",
                "weeks" => "DurationWeeks",
                "days" => "FormattedDays",
                "online" => "IsLive",
                "start date" => "Date",
                "start" => "StartAt",
                "room" => "RoomId",
                "max" => "MaxAttendees",
                _ => key
            };
        }
        private static DateTime? GetStartDateTime(CourseDTO e)
        {
            if (!DateTime.TryParseExact(e.Date ?? "", "dd.MM.yyyy", new CultureInfo("sr-RS"),
                                        DateTimeStyles.None, out var d) &&
                !DateTime.TryParseExact(e.Date ?? "", "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                        DateTimeStyles.None, out d))
                return null;

            if (!TimeSpan.TryParseExact(e.StartAt ?? "", @"hh\:mm", CultureInfo.InvariantCulture, out var ts))
                return null;

            return d.Date.Add(ts);
        }

        private void ApplyFiltersSortAndPageForCourses(bool resetPage = true)
        {
            // 1) filter
            _filteredCourses = ApplyCurrentFiltersForCourses(_allCourses);

            // 2) sort
            _sortedCourses = ApplySortForCourses(_filteredCourses, _sortKey, _sortDir).ToList();

            // 3) page
            var totalPages = Math.Max(1, (int)Math.Ceiling((double)_sortedCourses.Count / _pageSizeForCourses));
            if (resetPage)
                _currentPageForCourses = 1;
            _currentPageForCourses = Math.Min(Math.Max(_currentPageForCourses, 1), totalPages);

            var slice = _sortedCourses.Skip((_currentPageForCourses - 1) * _pageSizeForCourses).Take(_pageSizeForCourses).ToList();

            _pageOfCourses.Clear();
            foreach (var x in slice)
                _pageOfCourses.Add(x);

            if (CourseLanguageCombo.FindName("PagerTextCourses") is TextBlock pager)
                pager.Text = $"Page {_currentPageForCourses}/{totalPages}  ·  {(_sortedCourses.Count == 1 ? "1 item" : _sortedCourses.Count + " items")}";
        }

        public void CoursesGridSorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;

            foreach (var c in CoursesGrid.Columns)
                c.SortDirection = null;

            var key = e.Column.SortMemberPath ?? e.Column.Header?.ToString() ?? "Date";
            key = NormalizeSortKeyForCourses(key);

            if (_sortKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                _sortDir = _sortDir == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            else
            {
                _sortKey = key;
                _sortDir = ListSortDirection.Ascending;
            }

            e.Column.SortDirection = _sortDir;
            ApplyFiltersSortAndPageForCourses(resetPage: false);
        }

        public void PageSizeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CoursesGrid == null || !CoursesGrid.IsLoaded) return;

            if (sender is ComboBox cb &&
                cb.SelectedItem is ComboBoxItem it &&
                int.TryParse(it.Content?.ToString(), out var ps) &&
                ps > 0)
            {
                _pageSizeForCourses = ps;
                _currentPageForCourses = 1;
                ApplyFiltersSortAndPageForCourses(resetPage: true);
            }
        }

        public void PrevPageForCourses(object? sender, RoutedEventArgs e)
        {
            if (_currentPageForCourses > 1)
            {
                _currentPageForCourses--;
                ApplyFiltersSortAndPageForCourses(resetPage: false);
            }
        }

        public void NextPageForCourses(object? sender, RoutedEventArgs e)
        {
            var totalPages = Math.Max(1, (int)Math.Ceiling((double)_sortedCourses.Count / _pageSizeForCourses));
            if (_currentPageForCourses < totalPages)
            {
                _currentPageForCourses++;
                ApplyFiltersSortAndPageForCourses(resetPage: false);
            }
        }



    }
}
