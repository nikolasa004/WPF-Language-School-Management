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
using LangLang.DTO;

namespace LangLang.RoleWindows.StudentViewModules
{
    internal class StudentViewCourses
    {
        private readonly CourseController _courseController;

        private List<CourseDTO> _availableCourses;
        private List<CourseDTO> _filtered;   // after filters
        private List<CourseDTO> _sorted;     // after sorting
        private readonly ObservableCollection<CourseDTO> _page = new();

        private int _pageSize = 10;
        private int _currentPage = 1;
        private string _sortKey = "Date";
        private ListSortDirection _sortDir = ListSortDirection.Ascending;

        private ComboBox CourseLanguageCombo;
        private ComboBox CourseLevelCombo;
        private ComboBox CourseOnlineCombo;
        private TextBox CourseWeeksBox;
        private DatePicker CourseFromDate;
        private DatePicker CourseToDate;
        private DataGrid CoursesGrid;

        public StudentViewCourses(ComboBox examLanguageCombo, ComboBox examLevelCombo, ComboBox courseOnlineCombo, TextBox courseWeeksBox, DatePicker examFromDate, DatePicker examToDate, DataGrid examsGrid)
        {
            _courseController = new CourseController();

            CourseLanguageCombo = examLanguageCombo;
            CourseLevelCombo = examLevelCombo;
            CourseOnlineCombo = courseOnlineCombo;
            CourseWeeksBox = courseWeeksBox;
            CourseFromDate = examFromDate;
            CourseToDate = examToDate;
            CoursesGrid = examsGrid;
            InitCourseFilters();

            List<Course> allCourses = _courseController.GetAll();
            _availableCourses = GetAvailableCourses(allCourses);
            _filtered = _availableCourses;
            _sorted = ApplySort(_filtered, _sortKey, _sortDir).ToList();

            ApplyFiltersSortAndPage();

            CoursesGrid.ItemsSource = _page;
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
                CourseDTO courseDto= new CourseDTO(course);
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
            ApplyFiltersSortAndPage(resetPage: true);
        }

        private List<CourseDTO> ApplyCurrentFilters(List<CourseDTO> input)
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
            if(CourseOnlineCombo.SelectedItem is ComboBoxItem comboItem)
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

        private IEnumerable<CourseDTO> ApplySort(IEnumerable<CourseDTO> source, string sortKey, ListSortDirection dir)
        {
            var key = NormalizeSortKey(sortKey);

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

        private static string NormalizeSortKey(string key)
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

        private void ApplyFiltersSortAndPage(bool resetPage = true)
        {
            // 1) filter
            _filtered = ApplyCurrentFilters(_availableCourses);

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

            if (CourseLanguageCombo.FindName("PagerTextCourses") is TextBlock pager)
                pager.Text = $"Page {_currentPage}/{totalPages}  ·  {(_sorted.Count == 1 ? "1 item" : _sorted.Count + " items")}";
        }

        public void CoursesGridSorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;

            foreach (var c in CoursesGrid.Columns)
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
            if (CoursesGrid == null || !CoursesGrid.IsLoaded) return;

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