using LangLang.Controller.Requests;
using LangLang.Controller.TeachingActivities;
using LangLang.Controller.Users;
using LangLang.DTO;
using LangLang.Model.Enums;
using LangLang.Model.Requests;
using LangLang.Model.TeachingActivities;
using LangLang.View.StudentViewModules;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LangLang.View.StudentViewModules.Exams
{
    internal class StudentViewExams
    {
        // --- Controllers
        private readonly ExamTermsController _examController;
        private readonly ExamEnrollmentsController _enrollmentsController;
        private readonly StudentsController _studentsController;

        // --- Data buffers
        private List<ExamTermDTO> _availableExams;
        private List<ExamTermDTO> _filtered;   // after filters
        private List<ExamTermDTO> _sorted;     // after sorting
        private readonly ObservableCollection<ExamTermDTO> _page = new();

        // --- Paging / sorting state
        private int _pageSize = 10;
        private int _currentPage = 1;
        private string _sortKey = "Date";
        private ListSortDirection _sortDir = ListSortDirection.Ascending;

        // --- UI refs
        private readonly ComboBox ExamLanguageCombo;
        private readonly ComboBox ExamLevelCombo;
        private readonly DatePicker ExamFromDate;
        private readonly DatePicker ExamToDate;
        private readonly DataGrid ExamsGrid;

        private readonly string _studentId;

        public StudentViewExams(
            ExamTermsController examController,
            ExamEnrollmentsController enrollmentsController,
            StudentsController studentsController,
            ComboBox examLanguageCombo,
            ComboBox examLevelCombo,
            DatePicker examFromDate,
            DatePicker examToDate,
            DataGrid examsGrid,
            string studentId)
        {
            _examController = examController ?? new ExamTermsController();
            _enrollmentsController = enrollmentsController ?? new ExamEnrollmentsController();
            _studentsController = studentsController ?? new StudentsController();

            ExamLanguageCombo = examLanguageCombo;
            ExamLevelCombo = examLevelCombo;
            ExamFromDate = examFromDate;
            ExamToDate = examToDate;
            ExamsGrid = examsGrid;
            _studentId = studentId;

            InitExamFilters();

            _availableExams = new List<ExamTermDTO>();
            _filtered = new List<ExamTermDTO>();
            _sorted = new List<ExamTermDTO>();

            ExamsGrid.ItemsSource = _page;
        }

        // -------------------------
        // Init
        // -------------------------
        private void InitExamFilters()
        {
            if (ExamLanguageCombo.Items.Count == 0)
                ExamLanguageCombo.ItemsSource = Enum.GetValues(typeof(Language));

            if (ExamLevelCombo.Items.Count == 0)
                ExamLevelCombo.ItemsSource = Enum.GetValues(typeof(LanguageLevel));

            ExamLanguageCombo.SelectedIndex = -1;
            ExamLevelCombo.SelectedIndex = -1;
        }

        // -------------------------
        // Public entry points
        // -------------------------
        public void ShowAllExams()
        {
            var all = _examController.GetAll();
            _availableExams = GetAvailableExams(all);
            _filtered = _availableExams;
            _sorted = ApplySort(_filtered, _sortKey, _sortDir).ToList();
            ApplyFiltersSortAndPage();
        }

        public void ShowRequestedExams()
        {
            var requestedTermIds = _studentsController.GetRequestedExamTerms(_studentId);
            _availableExams = GetExamsByIds(requestedTermIds);
            _filtered = _availableExams;
            _sorted = ApplySort(_filtered, _sortKey, _sortDir).ToList();
            ApplyFiltersSortAndPage();
        }

        public void ShowActiveExam()
        {
            int termId = _studentsController.GetActiveExamTermId(_studentId);
            if (termId != -1)
            {
                var term = _examController.GetById(termId);
                _availableExams = term != null ? new List<ExamTermDTO> { new ExamTermDTO(term) }
                                               : new List<ExamTermDTO>();
            }
            else _availableExams = new List<ExamTermDTO>();

            _filtered = _availableExams;
            _sorted = ApplySort(_filtered, _sortKey, _sortDir).ToList();
            ApplyFiltersSortAndPage();
        }

        public void ShowFinishedExams()
        {
            var win = new StudentFinishedExamsWindow(_studentsController, _studentId);
            win.Show();
        }

        public void SearchExams() => ApplyFiltersSortAndPage(resetPage: true);

        // -------------------------
        // Helpers (load/filter/sort)
        // -------------------------
        private List<ExamTermDTO> GetExamsByIds(List<int> examIds)
        {
            var exams = new List<ExamTermDTO>();
            foreach (int id in examIds)
            {
                var exam = _examController.GetById(id);
                if (exam != null) exams.Add(new ExamTermDTO(exam));
            }
            return exams;
        }

        private List<ExamTermDTO> ApplyCurrentFilters(List<ExamTermDTO> input)
        {
            Language? lang = ExamLanguageCombo.SelectedItem is Language l ? l : null;
            LanguageLevel? level = ExamLevelCombo.SelectedItem is LanguageLevel lev ? lev : null;

            DateTime? from = ExamFromDate.SelectedDate?.Date;
            DateTime? to = ExamToDate.SelectedDate?.Date;
            if (from.HasValue && to.HasValue && from.Value > to.Value) (from, to) = (to, from);

            return GetFilterResults(input, lang, level, from, to);
        }

        private List<ExamTermDTO> GetFilterResults(List<ExamTermDTO> input, Language? lang, LanguageLevel? level, DateTime? from, DateTime? to)
        {
            var inputRaw = input.Select(dto => dto.ToExamTerm()).ToList();
            var resultRaw = _examController.SearchExamTerms(inputRaw, lang, level, from, to);
            return resultRaw.Select(exam => new ExamTermDTO(exam)).ToList();
        }

        private IEnumerable<ExamTermDTO> ApplySort(IEnumerable<ExamTermDTO> source, string sortKey, ListSortDirection direction)
        {
            var key = NormalizeSortKey(sortKey);
            Func<ExamTermDTO, object> keySelector = key switch
            {
                "Id" => exam => exam.Id,
                "Language" => exam => exam.Language.ToString(),
                "Level" => exam => (int)exam.Level,
                "RoomId" => exam => exam.RoomId ?? string.Empty,
                "MaxAttendees" => exam => exam.MaxAttendees,
                "StartAt" => exam => exam.StartAt ?? string.Empty,
                "Date" => exam => GetStartDateTime(exam) ?? DateTime.MinValue,
                _ => exam => GetStartDateTime(exam) ?? DateTime.MinValue
            };
            return direction == ListSortDirection.Ascending ? source.OrderBy(keySelector)
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

        private static DateTime? GetStartDateTime(ExamTermDTO exam)
        {
            if (!DateTime.TryParseExact(exam.Date ?? string.Empty, "dd.MM.yyyy",
                    new CultureInfo("sr-RS"), DateTimeStyles.None, out var date) &&
                !DateTime.TryParseExact(exam.Date ?? string.Empty, "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                return null;

            if (!TimeSpan.TryParseExact(exam.StartAt ?? string.Empty, @"hh\:mm",
                    CultureInfo.InvariantCulture, out var timeSpan))
                return null;

            return date.Date.Add(timeSpan);
        }

        private void ApplyFiltersSortAndPage(bool resetPage = true)
        {
            _filtered = ApplyCurrentFilters(_availableExams);
            _sorted = ApplySort(_filtered, _sortKey, _sortDir).ToList();

            var totalPages = Math.Max(1, (int)Math.Ceiling((double)_sorted.Count / _pageSize));
            if (resetPage) _currentPage = 1;
            _currentPage = Math.Min(Math.Max(_currentPage, 1), totalPages);

            var slice = _sorted.Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();

            _page.Clear();
            foreach (var exam in slice) _page.Add(exam);

            if (ExamLanguageCombo.FindName("PagerTextExams") is TextBlock pager)
                pager.Text = $"Page {_currentPage}/{totalPages}  ·  {(_sorted.Count == 1 ? "1 item" : _sorted.Count + " items")}";
        }

        // -------------------------
        // Sorting (WPF event)
        // -------------------------
        public void ExamsGridSorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;

            foreach (var column in ExamsGrid.Columns) column.SortDirection = null;

            var key = NormalizeSortKey(e.Column.SortMemberPath ?? e.Column.Header?.ToString() ?? "Date");
            if (_sortKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                _sortDir = _sortDir == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            else { _sortKey = key; _sortDir = ListSortDirection.Ascending; }

            e.Column.SortDirection = _sortDir;
            ApplyFiltersSortAndPage(resetPage: false);
        }

        // -------------------------
        // Paging handlers
        // -------------------------
        public void PageSizeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ExamsGrid == null || !ExamsGrid.IsLoaded) return;

            if (sender is ComboBox comboBox &&
                comboBox.SelectedItem is ComboBoxItem item &&
                int.TryParse(item.Content?.ToString(), out var pageSize) &&
                pageSize > 0)
            {
                _pageSize = pageSize;
                _currentPage = 1;
                ApplyFiltersSortAndPage(resetPage: true);
            }
        }

        public void PrevPage()
        {
            if (_currentPage <= 1) return;
            _currentPage--;
            ApplyFiltersSortAndPage(resetPage: false);
        }

        public void NextPage()
        {
            var totalPages = Math.Max(1, (int)Math.Ceiling((double)_sorted.Count / _pageSize));
            if (_currentPage >= totalPages) return;
            _currentPage++;
            ApplyFiltersSortAndPage(resetPage: false);
        }

        // -------------------------
        // Actions: APPLY / CANCEL (split to helpers)
        // -------------------------
        public void RequestForExam()
        {
            var selectedExam = GetSelectedExamOrWarn();
            if (selectedExam == null) return;
            if (!CanApplyToExam(selectedExam)) return;
            if (!CreateEnrollment(selectedExam)) return;
            AfterApplySuccess(selectedExam);
        }

        public void CancelRequestForExam()
        {
            var selectedExam = GetSelectedExamOrWarn();
            if (selectedExam == null) return;
            if (!CanCancel(selectedExam)) return;

            var enrollment = FindEnrollmentForSelected(selectedExam.Id);
            if (enrollment == null)
            {
                MessageBox.Show("Application record not found for this exam term.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!_enrollmentsController.CancelEnrollment(enrollment.ID))
            {
                MessageBox.Show("Too late to cancel this application.", "Cancellation Not Allowed",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _studentsController.RemoveExamRequest(_studentId, selectedExam.Id);
            MessageBox.Show("Cancelled exam application successfully.", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);

            if (!_availableExams.Any(e => e.Id == selectedExam.Id))
                _availableExams.Add(selectedExam);

            ApplyFiltersSortAndPage(resetPage: true);
        }

        // ---------- helpers for APPLY ----------
        private ExamTermDTO? GetSelectedExamOrWarn()
        {
            if (ExamsGrid.SelectedItem is ExamTermDTO sel) return sel;
            MessageBox.Show("Please select an exam term first.", "No Selection",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }

        private bool CanApplyToExam(ExamTermDTO term)
        {
            var s = _studentsController.GetStudentById(_studentId);
            if (s?.IsExamApplicationLocked == true)
            {
                MessageBox.Show("Exam applications are temporarily locked. Please wait for the director's decision.",
                    "Locked", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (_studentsController.GetActiveExamTermId(_studentId) != -1)
            {
                MessageBox.Show("You already have an active exam application.",
                    "Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            var start = GetStartDateTime(term);
            if (!start.HasValue || start.Value <= DateTime.Now)
            {
                MessageBox.Show("You can only apply to future exam terms.",
                    "Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            var raw = _examController.GetById(term.Id);
            if (raw == null || CountEnrolled(raw.Id) >= raw.MaxAttendees)
            {
                MessageBox.Show("This exam term is full.", "No seats",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private bool CreateEnrollment(ExamTermDTO term)
        {
            int? newId = _enrollmentsController.Add(new ExamEnrollment(
                0, _studentId, term.Id, DateTime.Now.ToString("yyyy-MM-dd"), RequestStatus.Pending));

            if (newId == null)
            {
                MessageBox.Show("Could not create application (term may be full).",
                    "Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void AfterApplySuccess(ExamTermDTO term)
        {
            _studentsController.AddExamRequest(_studentId, term.Id);
            MessageBox.Show("Exam application submitted successfully!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);

            _availableExams.RemoveAll(e => e.Id == term.Id);
            ApplyFiltersSortAndPage(resetPage: true);
        }

        // ---------- helpers for CANCEL ----------
        private bool CanCancel(ExamTermDTO term)
        {
            if (GetStartDateTime(term) is DateTime start && start < DateTime.Now.AddDays(2))
            {
                MessageBox.Show("You can cancel only up to 2 days before the exam.",
                    "Cancellation Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private ExamEnrollment? FindEnrollmentForSelected(int termId)
        {
            return _enrollmentsController.GetAllExamEnrollments()
                .FirstOrDefault(e => e.TeachingActivityID == termId && e.StudentID == _studentId);
        }

        // -------------------------
        // Availability (finished courses + capacity)
        // -------------------------
        private int CountEnrolled(int termId)
        {
            return _enrollmentsController.GetAllExamEnrollments()
                .Count(e => e.TeachingActivityID == termId);
        }

        private List<ExamTermDTO> GetAvailableExams(List<ExamTerm> allExams)
        {
            var now = DateTime.Now;

            var finishedCourses = _studentsController.GetFinishedCourses(_studentId);
            var allowed = new HashSet<(Language, LanguageLevel)>(
                finishedCourses.Select(t => (t.Item1, t.Item2)));

            var requestedTermIds = _studentsController.GetRequestedExamTerms(_studentId);

            var available = new List<ExamTermDTO>();
            foreach (var exam in allExams)
            {
                var dto = new ExamTermDTO(exam);
                var start = GetStartDateTime(dto);
                if (!start.HasValue || start.Value <= now) continue;
                if (!allowed.Contains((exam.Language, exam.Level))) continue;
                if (requestedTermIds.Contains(exam.Id)) continue;
                if (CountEnrolled(exam.Id) >= exam.MaxAttendees) continue;
                available.Add(dto);
            }
            return available;
        }
    }
}
