using LangLang.Controller.TeachingActivities;
using LangLang.DTO;
using LangLang.Model.Enums;
using LangLang.Model.TeachingActivities;
using LangLang.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

namespace LangLang.View.StudentViewModules.Courses
{
    class StudentSortCourses
    {
        private readonly CourseController _courseController;
        private List<CourseDTO> _availableCourses;
        private List<CourseDTO> _filtered;
        private List<CourseDTO> _sorted;
        private ComboBox CourseLanguageCombo;
        private ComboBox CourseLevelCombo;
        private ComboBox CourseOnlineCombo;
        private TextBox CourseWeeksBox;
        private DatePicker CourseFromDate;
        private DatePicker CourseToDate;
        public DataGrid CoursesGrid;
        private readonly ObservableCollection<CourseDTO> _page;

        private int _pageSize;
        private int _currentPage;
        private string _sortKey;
        private ListSortDirection _sortDir;

        public StudentSortCourses(CourseController courseController, List<CourseDTO> availableCourses, List<CourseDTO> filteredCourses, List<CourseDTO> sortedCourses, ComboBox courseLanguageCombo, ComboBox courseLevelCombo, ComboBox courseOnlineCombo, TextBox courseWeeksBox, DatePicker courseFromDate, DatePicker courseToDate, DataGrid coursesGrid, ObservableCollection<CourseDTO> page, int pageSize, int currentPage, string sortKey, ListSortDirection sortDir)
        {
            _courseController = courseController;
            _availableCourses = availableCourses;
            CourseLanguageCombo = courseLanguageCombo;
            CourseLevelCombo = courseLevelCombo;
            CourseOnlineCombo = courseOnlineCombo;
            CourseWeeksBox = courseWeeksBox;
            CourseFromDate = courseFromDate;
            CourseToDate = courseToDate;
            _filtered = filteredCourses;
            _sorted = sortedCourses;
            _page = page;
            CoursesGrid = coursesGrid;
            _pageSize = pageSize;
            _currentPage = currentPage;
            _sortKey = sortKey;
            _sortDir = sortDir;
        }

        public List<CourseDTO> ApplyCurrentFilters(List<CourseDTO> input)
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
        private List<CourseDTO> GetFilterResults(List<CourseDTO> input, Language? lang, LanguageLevel? level, bool? isLive, int weeks, DateTime? from, DateTime? to)
        {
            List<Course> inputRaw = input.Select(dto => dto.ToCourse()).ToList();
            List<Course> resultRaw = _courseController.SearchCourses(inputRaw, lang, level, isLive, weeks, from, to);
            List<CourseDTO> result = resultRaw.Select(course => new CourseDTO(course)).ToList();
            return result;
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
        public void ApplyFiltersSortAndPage(bool resetPage = true)
        {
            _filtered.Clear();
            _filtered.AddRange(ApplyCurrentFilters(_availableCourses));
            _sorted.Clear();
            _sorted.AddRange(GUIHelper.SortCourses(_filtered, _sortKey, _sortDir));

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
        public void CoursesGridSorting(DataGridSortingEventArgs e)
        {
            e.Handled = true;
            foreach (var column in CoursesGrid.Columns)
                column.SortDirection = null;

            var key = e.Column.SortMemberPath ?? e.Column.Header?.ToString() ?? "Date";
            key = GUIHelper.NormalizeCourseKey(key);
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
        public void PageSizeChanged(object sender)
        {
            if (CoursesGrid == null || !CoursesGrid.IsLoaded) return;

            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem item && int.TryParse(item.Content?.ToString(), out var pageSize) && pageSize > 0)
            {
                _pageSize = pageSize;
                _currentPage = 1;
                ApplyFiltersSortAndPage(resetPage: true);
            }
        }
    }
}