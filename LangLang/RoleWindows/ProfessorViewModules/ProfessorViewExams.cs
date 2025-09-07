using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using LangLang.Controller;
using LangLang.DTO;
using LangLang.Model.Enums;

namespace LangLang.RoleWindows.ProfessorExamModules
{
    public class ProfessorViewExams
    {
        private readonly ExamTermsController _examController;
        private readonly string _professorId;
        private readonly DataGrid _grid;
        private readonly ComboBox _langCombo, _levelCombo;
        private readonly DatePicker _fromDate, _toDate;
        private readonly TextBlock _pagerText;
        private readonly ComboBox _pageSizeCombo;
        private List<ExamTermDTO> _all = new();       
        private List<ExamTermDTO> _filtered = new();  
        private List<ExamTermDTO> _sorted = new();    
        private readonly ObservableCollection<ExamTermDTO> _page = new(); 
        private int _pageSize = 10;
        private int _currentPage = 1;
        private string _sortKey = "Date";
        private ListSortDirection _sortDir = ListSortDirection.Ascending;
        public ProfessorViewExams(
            string professorId,
            ExamTermsController examController,
            DataGrid grid,
            ComboBox langCombo, ComboBox levelCombo,
            DatePicker fromDate, DatePicker toDate,
            TextBlock pagerText,
            ComboBox pageSizeCombo)
        {
            _professorId = professorId;
            _examController = examController;
            _grid = grid;
            _langCombo = langCombo; _levelCombo = levelCombo;
            _fromDate = fromDate; _toDate = toDate;
            _pagerText = pagerText; _pageSizeCombo = pageSizeCombo;
            _grid.ItemsSource = _page;
        }
        public void InitFilters(CultureInfo culture, XmlLanguage xamlLang)
        {
            if (_langCombo.Items.Count == 0) _langCombo.ItemsSource = Enum.GetValues(typeof(Language));
            if (_levelCombo.Items.Count == 0) _levelCombo.ItemsSource = Enum.GetValues(typeof(LanguageLevel));
            _langCombo.SelectedIndex = -1;
            _levelCombo.SelectedIndex = -1;
            if (_fromDate != null)
            {
                _fromDate.Language = xamlLang;
                _fromDate.SelectedDateFormat = DatePickerFormat.Short;
                _fromDate.SelectedDate = null;
            }
            if (_toDate != null)
            {
                _toDate.Language = xamlLang;
                _toDate.SelectedDateFormat = DatePickerFormat.Short;
                _toDate.SelectedDate = null;
            }
        }
        public void ReloadAll()
        {
            _all = _examController
                .GetMyExamTerms(_professorId)
                .Select(e => new ExamTermDTO(e))
                .ToList();

            ApplyFiltersSortAndPage();
        }
        public void ApplyFiltersSortAndPage(bool resetPage = true)
        {
            _filtered = ApplyCurrentFilters(_all);
            _sorted = ApplySort(_filtered, _sortKey, _sortDir).ToList();
            var totalPages = Math.Max(1, (int)Math.Ceiling((double)_sorted.Count / _pageSize));
            if (resetPage) _currentPage = 1;
            _currentPage = Math.Min(Math.Max(_currentPage, 1), totalPages);
            var slice = _sorted.Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();
            _page.Clear();
            foreach (var x in slice) _page.Add(x);
            if (_pagerText != null)
                _pagerText.Text = $"Page {_currentPage}/{totalPages}  ·  {(_sorted.Count == 1 ? "1 item" : _sorted.Count + " items")}";
        }
        private List<ExamTermDTO> ApplyCurrentFilters(List<ExamTermDTO> input)
        {
            Language? lang = _langCombo.SelectedItem is Language l ? l : null;
            LanguageLevel? level = _levelCombo.SelectedItem is LanguageLevel lv ? lv : null;
            DateTime? from = _fromDate.SelectedDate?.Date;
            DateTime? to = _toDate.SelectedDate?.Date;
            if (from.HasValue && to.HasValue && from.Value > to.Value) (from, to) = (to, from);
            var q = input.AsEnumerable();
            if (lang.HasValue)
                q = q.Where(d => d.Language == lang.Value);
            if (level.HasValue)
                q = q.Where(d => d.Level == level.Value);
            if (from.HasValue || to.HasValue)
            {
                q = q.Where(d =>
                {
                    if (!d.TryGetStart(out var start)) return false;
                    var day = start.Date;
                    if (from.HasValue && day < from.Value) return false;
                    if (to.HasValue && day > to.Value) return false;
                    return true;
                });
            }
            return q.ToList();
        }
        public void GridSorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;
            foreach (var c in _grid.Columns) c.SortDirection = null;
            var key = e.Column.SortMemberPath ?? e.Column.Header?.ToString() ?? "Date";
            key = NormalizeSortKey(key);
            if (_sortKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                _sortDir = _sortDir == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            else { _sortKey = key; _sortDir = ListSortDirection.Ascending; }
            e.Column.SortDirection = _sortDir;
            ApplyFiltersSortAndPage(resetPage: false);
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
        public void PageSizeChanged(object sender, SelectionChangedEventArgs e, bool isWindowLoaded)
        {
            if (!isWindowLoaded) return;
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
        private static IEnumerable<ExamTermDTO> ApplySort(IEnumerable<ExamTermDTO> source, string sortKey, ListSortDirection dir)
        {
            var key = NormalizeSortKey(sortKey);
            Func<ExamTermDTO, object> keySelector = key switch
            {
                "Id" => d => d.Id,
                "Language" => d => d.Language.ToString(),
                "Level" => d => (int)d.Level,
                "RoomId" => d => d.RoomId ?? "",
                "MaxAttendees" => d => d.MaxAttendees,
                "StartAt" => d => (d.TryGetStart(out var s1) ? s1 : DateTime.MinValue),
                "Date" => d => (d.TryGetStart(out var s2) ? s2 : DateTime.MinValue),
                _ => d => (d.TryGetStart(out var s3) ? s3 : DateTime.MinValue)
            };
            return dir == ListSortDirection.Ascending
                ? source.OrderBy(keySelector)
                : source.OrderByDescending(keySelector);
        }
        public static bool TryParseExamDateString(string? s, out DateTime date)
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
        public static bool TryGetExamStart(ExamTermDTO dto, out DateTime start)
        {
            return dto.TryGetStart(out start);
        }
    }
}
