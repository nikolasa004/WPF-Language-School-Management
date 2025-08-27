using LangLang.Controller;
using LangLang.Model;
using LangLang.Model.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LangLang.RoleWindows.StudentViewModules
{
    internal class StudentViewExams
    {
        private readonly ExamTermsController _examController;

        private List<ExamTerm> _availableExams;
        private List<ExamTerm> _filtered;   // after filters
        private List<ExamTerm> _sorted;     // after sorting
        private readonly ObservableCollection<ExamTerm> _page = new();

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

        public void ComboBoxDeselect(object sender, MouseButtonEventArgs e)
        {
            var item = (ComboBoxItem)sender;
            var combo = ItemsControl.ItemsControlFromItemContainer(item) as ComboBox;
            if (combo == null) return;

            var clickedDataItem = combo.ItemContainerGenerator.ItemFromContainer(item);

            if (Equals(combo.SelectedItem, clickedDataItem))
            {
                combo.SelectedIndex = -1;
                combo.IsDropDownOpen = false;
                e.Handled = true;
            }
        }

        private List<ExamTerm> GetAvailableExams(List<ExamTerm> allExams)
        {
            var now = DateTime.Now;
            List<ExamTerm> available = new();
            foreach (var exam in allExams)
            {
                /////////////////To add
                //language and level match
                //exam not full
                var start = GetStartDateTime(exam);
                if (start.HasValue && start.Value > now.AddDays(30))
                {
                    available.Add(exam);
                }
            }

            return available;
        }

        public void SearchExams(object sender, RoutedEventArgs e)
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


            return _examController.SearchExamTerms(input,lang, level, from, to);
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

        private void ApplyFiltersSortAndPage(bool resetPage = true)
        {
            // 1) filter
            _filtered = ApplyCurrentFilters(_availableExams);

            // 2) sort
            _sorted = ApplySort(_filtered, _sortKey, _sortDir).ToList();

            // 3) page
            var totalPages = Math.Max(1, (int)Math.Ceiling((double)_sorted.Count / _pageSize));
            if (resetPage)
                _currentPage = 1;
            _currentPage = Math.Min(Math.Max(_currentPage, 1), totalPages);

            var slice = _sorted.Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();

            _page.Clear();
            foreach (var x in slice)
                _page.Add(x);

            if (ExamLanguageCombo.FindName("PagerText") is TextBlock pager)
                pager.Text = $"Page {_currentPage}/{totalPages}  ·  {(_sorted.Count == 1 ? "1 item" : _sorted.Count + " items")}";
        }

        public void ExamsGridSorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;

            foreach (var c in ExamsGrid.Columns)
                c.SortDirection = null;

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
