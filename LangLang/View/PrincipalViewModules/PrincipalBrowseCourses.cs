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
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using LangLang.Utils;
namespace LangLang.View.PrincipalViewModules
{
    internal class PrincipalBrowseCourses
    {
        private readonly CourseController _courseController;
        private List<CourseDTO> _allCourses;
        private List<CourseDTO> _filtered;
        private List<CourseDTO> _sorted;
        private readonly ObservableCollection<CourseDTO> _page = new();
        private int _pageSize = 10;
        private int _pageIndex = 1;
        private string _sortKey = "Date";
        private ListSortDirection _sortDir = ListSortDirection.Ascending;
        private readonly ComboBox _lang;
        private readonly ComboBox _level;
        private readonly ComboBox _online;
        private readonly TextBox _weeks;
        private readonly DatePicker _from;
        private readonly DatePicker _to;
        private readonly DataGrid _grid;
        private readonly TextBlock _pager;
        public PrincipalBrowseCourses(
            ComboBox lang, ComboBox level, ComboBox online,
            TextBox weeks, DatePicker from, DatePicker to,
            DataGrid grid, TextBlock pager)
        {
            _courseController = new CourseController();
            _lang = lang;
            _level = level;
            _online = online;
            _weeks = weeks;
            _from = from;
            _to = to;
            _grid = grid;
            _pager = pager;
            _allCourses = _courseController.GetAll().Select(c => new CourseDTO(c)).ToList();
            _filtered = _allCourses;
            _sorted = _allCourses;
            InitFilters();
            _grid.ItemsSource = _page;
            Apply(resetPage: true);
        }
        public void InitFilters()
        {
            if (_lang.Items.Count == 0) _lang.ItemsSource = Enum.GetValues(typeof(Language));
            if (_level.Items.Count == 0) _level.ItemsSource = Enum.GetValues(typeof(LanguageLevel));
            _lang.SelectedIndex = -1;
            _level.SelectedIndex = -1;
        }
        public void Refresh(bool resetPage = true)
        {
            _allCourses = _courseController.GetAll().Select(c => new CourseDTO(c)).ToList();
            Apply(resetPage);
        }
        public void SearchCourses(object? sender, RoutedEventArgs e) => Apply(resetPage: true);
        public void CoursesGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;

            foreach (var c in _grid.Columns) c.SortDirection = null;

            var key = e.Column.SortMemberPath ?? e.Column.Header?.ToString() ?? "Date";
            key = NormalizeKey(key);

            if (string.Equals(_sortKey, key, StringComparison.OrdinalIgnoreCase))
                _sortDir = _sortDir == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            else { _sortKey = key; _sortDir = ListSortDirection.Ascending; }

            e.Column.SortDirection = _sortDir;
            Apply(resetPage: false);
        }
        public void PrevPage() { if (_pageIndex > 1) { _pageIndex--; Apply(resetPage: false); } }
        public void NextPage()
        {
            var total = Math.Max(1, (int)Math.Ceiling((double)_sorted.Count / _pageSize));
            if (_pageIndex < total) { _pageIndex++; Apply(resetPage: false); }
        }
        public void ChangePageSize(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox cb && cb.SelectedItem is ComboBoxItem it &&
                int.TryParse(it.Content?.ToString(), out var size) && size > 0)
            {
                _pageSize = size;
                _pageIndex = 1;
                Apply(resetPage: true);
            }
        }
        private void Apply(bool resetPage)
        {
            _filtered = ApplyFilters(_allCourses);
            _sorted = ApplySort(_filtered, _sortKey, _sortDir).ToList();
            var totalPages = Math.Max(1, (int)Math.Ceiling((double)_sorted.Count / _pageSize));
            if (resetPage) _pageIndex = 1;
            _pageIndex = Math.Min(Math.Max(_pageIndex, 1), totalPages);
            var slice = _sorted.Skip((_pageIndex - 1) * _pageSize).Take(_pageSize).ToList();
            _page.Clear();
            foreach (var c in slice) _page.Add(c);
            if (_pager != null)
                _pager.Text = $"Page {_pageIndex}/{totalPages}  ·  {(_sorted.Count == 1 ? "1 item" : _sorted.Count + " items")}";
        }
        private List<CourseDTO> ApplyFilters(List<CourseDTO> input)
        {
            Language? lang = _lang.SelectedItem is Language l ? l : null;
            LanguageLevel? level = _level.SelectedItem is LanguageLevel lv ? lv : null;
            DateTime? from = _from.SelectedDate?.Date;
            DateTime? to = _to.SelectedDate?.Date;
            if (from.HasValue && to.HasValue && from.Value > to.Value) (from, to) = (to, from);
            bool? isLive = null;
            if (_online.SelectedItem is ComboBoxItem item)
            {
                var s = item.Content?.ToString();
                if (s == "Yes") isLive = true;
                else if (s == "No") isLive = false;
            }
            int weeks = 0;
            if (int.TryParse(_weeks.Text, out var w) && w > 0) weeks = w;
            var filter = new CoursesSearchFilter(lang, level, isLive, weeks);
            List<Course> raw = input.Select(d => d.ToCourse()).ToList();
            var res = _courseController.SearchCourses(raw, filter, from, to);
            return res.Select(c => new CourseDTO(c)).ToList();
        }
        private IEnumerable<CourseDTO> ApplySort(IEnumerable<CourseDTO> source, string key, ListSortDirection dir)
        {
            key = NormalizeKey(key);

            Func<CourseDTO, object> sel = key switch
            {
                "Id" => e => e.Id,
                "Language" => e => e.Language.ToString(),
                "Level" => e => (int)e.Level,
                "ProfessorId" => e => e.ProfessorId ?? "",
                "DurationWeeks" => e => e.DurationWeeks,
                "DaysFormatted" => e => e.DaysFormatted ?? "",
                "IsLive" => e => e.IsLive,
                "RoomId" => e => e.RoomId ?? "",
                "MaxAttendees" => e => e.MaxAttendees ?? 0,
                "StartAt" => e => e.StartAt ?? "",
                "Date" => e => StartDateTime(e) ?? DateTime.MinValue,
                _ => e => StartDateTime(e) ?? DateTime.MinValue
            };

            return dir == ListSortDirection.Ascending ? source.OrderBy(sel) : source.OrderByDescending(sel);
        }

        private static string NormalizeKey(string key)
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
                "days" => "DaysFormatted",
                "online" => "IsLive",
                "start date" => "Date",
                "start" => "StartAt",
                "room" => "RoomId",
                "max" => "MaxAttendees",
                _ => key
            };
        }

        private static DateTime? StartDateTime(CourseDTO c)
        {
            if (!DateTime.TryParseExact(c.Date ?? "", "dd.MM.yyyy", new CultureInfo("sr-RS"), DateTimeStyles.None, out var date) &&
                !DateTime.TryParseExact(c.Date ?? "", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                return null;
            if (!TimeSpan.TryParseExact(c.StartAt ?? "", @"hh\:mm", CultureInfo.InvariantCulture, out var t))
                return null;
            return date.Date.Add(t);
        }
    }
}
