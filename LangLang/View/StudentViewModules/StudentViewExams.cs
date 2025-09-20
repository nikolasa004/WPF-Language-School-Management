using LangLang.Controller.TeachingActivities;
using LangLang.DTO;
using LangLang.Model.Enums;
using LangLang.Model.TeachingActivities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace LangLang.RoleWindows.StudentViewModules
{
    internal class StudentViewExams
    {
        private readonly ExamTermsController _examController;

        private List<ExamTermDTO> _availableExams;
        private List<ExamTermDTO> _filtered;   // after filters
        private List<ExamTermDTO> _sorted;     // after sorting
        private readonly ObservableCollection<ExamTermDTO> _page = new();

        private int _pageSize = 10;
        private int _currentPage = 1;
        private string _sortKey = "Date";
        private ListSortDirection _sortDir = ListSortDirection.Ascending;

        private ComboBox ExamLanguageCombo;
        private ComboBox ExamLevelCombo;
        private DatePicker ExamFromDate;
        private DatePicker ExamToDate;
        private DataGrid ExamsGrid;

        public StudentViewExams(ComboBox examLanguageCombo, ComboBox examLevelCombo, DatePicker examFromDate, DatePicker examToDate, DataGrid examsGrid)
        {
            _examController = new ExamTermsController();

            ExamLanguageCombo = examLanguageCombo;
            ExamLevelCombo = examLevelCombo;
            ExamFromDate = examFromDate;
            ExamToDate = examToDate;
            ExamsGrid = examsGrid;
            InitExamFilters();

            List<ExamTerm> allExams = _examController.GetAll();
            _availableExams = GetAvailableExams(allExams);
            _filtered = _availableExams;
            _sorted = ApplySort(_filtered, _sortKey, _sortDir).ToList();

            ApplyFiltersSortAndPage();

            ExamsGrid.ItemsSource = _page;
        }

        private void InitExamFilters()
        {
            if (ExamLanguageCombo.Items.Count == 0)
                ExamLanguageCombo.ItemsSource = Enum.GetValues(typeof(Language));

            if (ExamLevelCombo.Items.Count == 0)
                ExamLevelCombo.ItemsSource = Enum.GetValues(typeof(LanguageLevel));

            ExamLanguageCombo.SelectedIndex = -1;
            ExamLevelCombo.SelectedIndex = -1;
        }

        private List<ExamTermDTO> GetAvailableExams(List<ExamTerm> allExams)
        {
            var now = DateTime.Now;
            List<ExamTermDTO> available = new();
            foreach (var exam in allExams)
            {
                /////////////////To add
                //language and level match
                //exam not full
                ExamTermDTO examDto = new(exam);
                var start = GetStartDateTime(examDto);
                if (start.HasValue && start.Value > now.AddDays(30))
                    available.Add(examDto);
            }

            return available;
        }

        public void SearchExams(object sender, RoutedEventArgs e)
        {
            ApplyFiltersSortAndPage(resetPage: true);
        }

        private List<ExamTermDTO> ApplyCurrentFilters(List<ExamTermDTO> input)
        {
            Language? lang = null;
            LanguageLevel? level = null;

            if (ExamLanguageCombo.SelectedItem is Language selLang) lang = selLang;
            if (ExamLevelCombo.SelectedItem is LanguageLevel selLevel) level = selLevel;

            DateTime? from = ExamFromDate.SelectedDate?.Date;
            DateTime? to = ExamToDate.SelectedDate?.Date;

            if (from.HasValue && to.HasValue && from.Value > to.Value)
                (from, to) = (to, from);

            return GetFilterResults(input, lang, level, from, to);
        }
        private List<ExamTermDTO> GetFilterResults(List<ExamTermDTO> input, Language? lang, LanguageLevel? level, DateTime? from, DateTime? to)
        {
            List<ExamTerm> inputRaw = input.Select(dto => dto.ToExamTerm()).ToList();
            List<ExamTerm> resultRaw = _examController.SearchExamTerms(inputRaw, lang, level, from, to);
            List<ExamTermDTO> result = resultRaw.Select(exam => new ExamTermDTO(exam)).ToList();
            return result;
        }

        private IEnumerable<ExamTermDTO> ApplySort(IEnumerable<ExamTermDTO> source, string sortKey, ListSortDirection direction)
        {

            var key = NormalizeSortKey(sortKey);

            Func<ExamTermDTO, object> keySelector = key switch
            {
                "Id" => exam => exam.Id,
                "Language" => exam => exam.Language.ToString(),
                "Level" => exam => (int)exam.Level,
                "RoomId" => exam => exam.RoomId ?? "",
                "MaxAttendees" => exam => exam.MaxAttendees, 
                "StartAt" => exam => exam.StartAt ?? "",
                "Date" => exam => GetStartDateTime(exam) ?? DateTime.MinValue,
                _ => exam => GetStartDateTime(exam) ?? DateTime.MinValue
            };

            return direction == ListSortDirection.Ascending ? source.OrderBy(keySelector) : source.OrderByDescending(keySelector);
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
            if (!DateTime.TryParseExact(exam.Date ?? "", "dd.MM.yyyy", new CultureInfo("sr-RS"), DateTimeStyles.None, out var date) && !DateTime.TryParseExact(exam.Date ?? "", "yyyy-MM-dd", CultureInfo.InvariantCulture,DateTimeStyles.None, out date))
                return null;

            if (!TimeSpan.TryParseExact(exam.StartAt ?? "", @"hh\:mm", CultureInfo.InvariantCulture, out var timeSpan))
                return null;

            return date.Date.Add(timeSpan);
        }

        private void ApplyFiltersSortAndPage(bool resetPage = true)
        {
            _filtered = ApplyCurrentFilters(_availableExams);
            _sorted = ApplySort(_filtered, _sortKey, _sortDir).ToList();

            var totalPages = Math.Max(1, (int)Math.Ceiling((double)_sorted.Count / _pageSize));
            if (resetPage)
                _currentPage = 1;
            _currentPage = Math.Min(Math.Max(_currentPage, 1), totalPages);

            var slice = _sorted.Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();

            _page.Clear();
            foreach (var exam in slice)
                _page.Add(exam);

            if (ExamLanguageCombo.FindName("PagerTextExams") is TextBlock pager)
                pager.Text = $"Page {_currentPage}/{totalPages}  ·  {(_sorted.Count == 1 ? "1 item" : _sorted.Count + " items")}";
        }

        public void ExamsGridSorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;

            foreach (var column in ExamsGrid.Columns)
                column.SortDirection = null;

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

        public void PageSizeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ExamsGrid == null || !ExamsGrid.IsLoaded) return;

            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem item && int.TryParse(item.Content?.ToString(), out var pageSize) && pageSize > 0)
            {
                _pageSize = pageSize;
                _currentPage = 1;
                ApplyFiltersSortAndPage(resetPage: true);
            }
        }
        public void PrevPage(object? sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                ApplyFiltersSortAndPage(resetPage: false);
            }
        }
        public void NextPage(object? sender, RoutedEventArgs e)
        {
            var totalPages = Math.Max(1, (int)Math.Ceiling((double)_sorted.Count / _pageSize));
            if (_currentPage < totalPages)
            {
                _currentPage++;
                ApplyFiltersSortAndPage(resetPage: false);
            }
        }
    }
}