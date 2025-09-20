using LangLang.Controller.Requests;
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
using LangLang.Model.Requests;

namespace LangLang.RoleWindows.StudentViewModules
{
    internal class StudentViewCourses
    {
        private readonly CourseController _courseController;
        private readonly CourseEnrollmentsController _enrollmentsController;

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
            _enrollmentsController = new CourseEnrollmentsController();
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

            bool? isLive = CheckLiveFilter();

            int weeks = 0;
            if (int.TryParse(CourseWeeksBox.Text, out var parsedWeeks) && parsedWeeks > 0)
                weeks = parsedWeeks;

            return GetFilterResults(input, lang, level, isLive, weeks, from, to);
        }
        private bool? CheckLiveFilter()
        {
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

            return isLive;
        }
        private List<CourseDTO> GetFilterResults(List<CourseDTO> input, Language? lang, LanguageLevel? level, bool? isLive, int weeks, DateTime? from, DateTime? to)
        {
            List<Course> inputRaw = input.Select(dto => dto.ToCourse()).ToList();
            List<Course> resultRaw = _courseController.SearchCourses(inputRaw, lang, level, isLive, weeks, from, to);
            List<CourseDTO> result = resultRaw.Select(course => new CourseDTO(course)).ToList();
            return result;
        }

        private IEnumerable<CourseDTO> ApplySort(IEnumerable<CourseDTO> source, string sortKey, ListSortDirection direction)
        {
            var key = NormalizeSortKey(sortKey);

            Func<CourseDTO, object> keySelector = key switch
            {
                "Id" => course => course.Id,
                "Language" => course => course.Language.ToString(),
                "Level" => course => (int)course.Level,
                "ProfessorId" => course => course.ProfessorId ?? "",
                "DurationWeeks" => course => course.DurationWeeks,
                "FormattedDays" => course => course.DaysFormatted ?? "",
                "IsLive" => course => course.IsLive,
                "RoomId" => course => course.RoomId ?? "",
                "MaxAttendees" => course => course.MaxAttendees ?? 0,
                "StartAt" => course => course.StartAt ?? "",
                "Date" => course => GetStartDateTime(course) ?? DateTime.MinValue,
                _ => course => GetStartDateTime(course) ?? DateTime.MinValue
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
            if (!DateTime.TryParseExact(course.Date ?? "", "dd.MM.yyyy", new CultureInfo("sr-RS"), DateTimeStyles.None, out var date) && !DateTime.TryParseExact(course.Date ?? "", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                return null;

            if (!TimeSpan.TryParseExact(course.StartAt ?? "", @"hh\:mm", CultureInfo.InvariantCulture, out var timeStamp))
                return null;

            return date.Date.Add(timeStamp);
        }

        private void ApplyFiltersSortAndPage(bool resetPage = true)
        {
            _filtered = ApplyCurrentFilters(_availableCourses);
            _sorted = ApplySort(_filtered, _sortKey, _sortDir).ToList();

            var totalPages = Math.Max(1, (int)Math.Ceiling((double)_sorted.Count / _pageSize));
            if (resetPage)
                _currentPage = 1;
            _currentPage = Math.Min(Math.Max(_currentPage, 1), totalPages);

            var slice = _sorted.Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();

            _page.Clear();
            foreach (var course in slice)
                _page.Add(course);

            if (CourseLanguageCombo.FindName("PagerTextCourses") is TextBlock pager)
                pager.Text = $"Page {_currentPage}/{totalPages}  ·  {(_sorted.Count == 1 ? "1 item" : _sorted.Count + " items")}";
        }

        public void CoursesGridSorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;

            foreach (var column in CoursesGrid.Columns)
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
            if (CoursesGrid == null || !CoursesGrid.IsLoaded) return;

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

        public void EnrollInSelectedCourse(object sender, RoutedEventArgs e, string studentId)
        {
            if (CoursesGrid.SelectedItem is CourseDTO selectedCourse)
            {
                _enrollmentsController.Add(new CourseEnrollment(0,studentId, selectedCourse.Id, DateTime.Now.ToString("yyyy-MM-dd"), RequestStatus.Pending));
                MessageBox.Show("Enrollment request submitted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                _availableCourses.RemoveAll(c => c.Id == selectedCourse.Id);
                ApplyFiltersSortAndPage(resetPage: true);
            }
            else
                MessageBox.Show("Please select a course to enroll in.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}