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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace LangLang.RoleWindows.ProfessorViewModules
{
    internal class ProfessorViewCourses
    {
        private readonly CourseController _courseController;

        private List<CourseDTO> _allCourses;
        private List<CourseDTO> _filteredCourses;
        private List<CourseDTO> _sortedCourses;
        private readonly ObservableCollection<CourseDTO> _pageOfCourses = new();

        private int _pageSizeForCourses = 10;
        private int _currentPageForCourses = 1;
        private string _sortKeyForCourses = "Date";
        private ListSortDirection _sortDir = ListSortDirection.Ascending;

        private ComboBox CourseLanguageCombo;
        private ComboBox CourseLevelCombo;
        private ComboBox CourseOnlineCombo;
        private TextBox CourseWeeksBox;
        private DatePicker CourseFromDate;
        private DatePicker CourseToDate;
        private DataGrid CoursesGrid;

        public ProfessorViewCourses(List<CourseDTO> sharedCourses, ComboBox courseLanguageCombo, ComboBox courseLevelCombo, ComboBox courseOnlineCombo,
                                    TextBox courseWeeksBox, DatePicker courseFromDate, DatePicker courseToDate, DataGrid coursesGrid)
        {
            _courseController = new CourseController();

            _allCourses = sharedCourses; 
            _filteredCourses = _allCourses;

            CourseLanguageCombo = courseLanguageCombo;
            CourseLevelCombo = courseLevelCombo;
            CourseOnlineCombo = courseOnlineCombo;
            CourseWeeksBox = courseWeeksBox;
            CourseFromDate = courseFromDate;
            CourseToDate = courseToDate;
            CoursesGrid = coursesGrid;

            InitCourseFilters();
            _allCourses = sharedCourses; 
            _filteredCourses = _allCourses;
            _sortedCourses = ApplySortForCourses(_filteredCourses, _sortKeyForCourses, _sortDir).ToList();

            ApplyFiltersSortAndPageForCourses();
            CoursesGrid.ItemsSource = _pageOfCourses;
        }

        public void InitCourseFilters()
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

        public List<CourseDTO> ApplyCurrentFiltersForCourses(List<CourseDTO> input)
        {
            Language? lang = null;
            LanguageLevel? level = null;

            if (CourseLanguageCombo.SelectedItem is Language selLang) lang = selLang;
            if (CourseLevelCombo.SelectedItem is LanguageLevel selLevel) level = selLevel;

            DateTime? from = CourseFromDate.SelectedDate?.Date;
            DateTime? to = CourseToDate.SelectedDate?.Date;
            if (from.HasValue && to.HasValue && from.Value > to.Value)
                (from, to) = (to, from);

            bool? isLive = CheckLiveFilter();

            int weeks = 0;
            if (int.TryParse(CourseWeeksBox.Text, out var parsedWeeks) && parsedWeeks > 0)
                weeks = parsedWeeks;

            return GetFilterResults(input, lang, level, isLive, weeks, from, to);
        }

        private bool? CheckLiveFilter()
        {
            if (CourseOnlineCombo.SelectedItem is ComboBoxItem comboItem)
            {
                var val = comboItem.Content?.ToString();
                if (val == "Yes") return true;
                if (val == "No") return false;
            }
            return null;
        }

        private List<CourseDTO> GetFilterResults(List<CourseDTO> input, Language? lang, LanguageLevel? level, bool? isLive, int weeks, DateTime? from, DateTime? to)
        {
            List<Course> inputRaw = input.Select(dto => dto.ToCourse()).ToList();
            List<Course> resultRaw = _courseController.SearchCourses(inputRaw, lang, level, isLive, weeks, from, to);
            return resultRaw.Select(course => new CourseDTO(course)).ToList();
        }

        private IEnumerable<CourseDTO> ApplySortForCourses(IEnumerable<CourseDTO> source, string sortKey, ListSortDirection direction)
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
                "StartAt" => e => e.StartAt ?? "",
                "Date" => e => GetStartDateTime(e) ?? DateTime.MinValue,
                _ => e => GetStartDateTime(e) ?? DateTime.MinValue
            };

            return direction == ListSortDirection.Ascending
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

        private static DateTime? GetStartDateTime(CourseDTO course)
        {
            if (!DateTime.TryParseExact(course.Date ?? "", "dd.MM.yyyy", new CultureInfo("sr-RS"), DateTimeStyles.None, out var date) &&
                !DateTime.TryParseExact(course.Date ?? "", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                return null;

            if (!TimeSpan.TryParseExact(course.StartAt ?? "", @"hh\:mm", CultureInfo.InvariantCulture, out var timeStamp))
                return null;

            return date.Date.Add(timeStamp);
        }

        public void ApplyFiltersSortAndPageForCourses(bool resetPage = true)
        {
            _filteredCourses = ApplyCurrentFiltersForCourses(_allCourses);
            _sortedCourses = ApplySortForCourses(_filteredCourses, _sortKeyForCourses, _sortDir).ToList();

            var totalPages = Math.Max(1, (int)Math.Ceiling((double)_sortedCourses.Count / _pageSizeForCourses));
            if (resetPage)
                _currentPageForCourses = 1;
            _currentPageForCourses = Math.Min(Math.Max(_currentPageForCourses, 1), totalPages);

            var slice = _sortedCourses.Skip((_currentPageForCourses - 1) * _pageSizeForCourses).Take(_pageSizeForCourses).ToList();

            _pageOfCourses.Clear();
            foreach (var course in slice)
                _pageOfCourses.Add(course);

            if (CourseLanguageCombo.FindName("PagerTextCourses") is TextBlock pager)
                pager.Text = $"Page {_currentPageForCourses}/{totalPages}  ·  {(_sortedCourses.Count == 1 ? "1 item" : _sortedCourses.Count + " items")}";
        }

        public void CoursesGridSorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;

            foreach (var column in CoursesGrid.Columns)
                column.SortDirection = null;

            var key = e.Column.SortMemberPath ?? e.Column.Header?.ToString() ?? "Date";
            key = NormalizeSortKeyForCourses(key);

            if (_sortKeyForCourses.Equals(key, StringComparison.OrdinalIgnoreCase))
                _sortDir = _sortDir == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            else
            {
                _sortKeyForCourses = key;
                _sortDir = ListSortDirection.Ascending;
            }

            e.Column.SortDirection = _sortDir;
            ApplyFiltersSortAndPageForCourses(resetPage: false);
        }

        public void PageSizeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CoursesGrid == null || !CoursesGrid.IsLoaded) return;

            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem item &&
                int.TryParse(item.Content?.ToString(), out var pageSize) && pageSize > 0)
            {
                _pageSizeForCourses = pageSize;
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

