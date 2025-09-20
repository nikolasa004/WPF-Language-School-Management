using LangLang.Controller.Messages;
using LangLang.Controller.Requests;
using LangLang.Controller.TeachingActivities;
using LangLang.Controller.Users;
using LangLang.DTO;
using LangLang.Model.Enums;
using LangLang.Model.Requests;
using LangLang.Model.TeachingActivities;
using LangLang.Model.Users;
using LangLang.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LangLang.RoleWindows.StudentViewModules
{
    internal class StudentViewCourses
    {
        private readonly CourseController _courseController;
        private readonly CourseEnrollmentsController _enrollmentsController;
        private readonly StudentsController _studentsController;
        private readonly MessagesController _messagesController;

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

        private string _studentId;

        public StudentViewCourses(CourseController courseController,ComboBox examLanguageCombo, ComboBox examLevelCombo, ComboBox courseOnlineCombo, TextBox courseWeeksBox, DatePicker examFromDate, DatePicker examToDate, DataGrid examsGrid, string studentId, StudentsController studentsController, MessagesController messagesController)
        {
            _courseController = courseController;
            _enrollmentsController = new CourseEnrollmentsController();
            _studentsController = studentsController;
            _messagesController = messagesController;
            CourseLanguageCombo = examLanguageCombo;
            CourseLevelCombo = examLevelCombo;
            CourseOnlineCombo = courseOnlineCombo;
            CourseWeeksBox = courseWeeksBox;
            CourseFromDate = examFromDate;
            CourseToDate = examToDate;
            CoursesGrid = examsGrid;
            _studentId = studentId;
            InitCourseFilters();

            _availableCourses = new List<CourseDTO>();
            _filtered = new List<CourseDTO>();
            _sorted = new List<CourseDTO>();

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

        public void ShowAllCourses()
        {
            List<Course> allCourses = _courseController.GetAll();
            _availableCourses = GetAvailableCourses(allCourses);
            _filtered = _availableCourses;
            _sorted = ApplySort(_filtered, _sortKey, _sortDir).ToList();
            ApplyFiltersSortAndPage();
        }
        private List<CourseDTO> GetAvailableCourses(List<Course> allCourses)
        {
            var now = DateTime.Now;
            List<CourseDTO> available = new();
            foreach (var course in allCourses)
            {
                /////////////////To add
                //course not full
                CourseDTO courseDto = new CourseDTO(course);
                var start = GetStartDateTime(courseDto);
                List<int> courseRequests=_studentsController.GetRequestedCourses(_studentId);
                List<int> requestedCourses = _enrollmentsController.GetCourseIds(courseRequests);
                if (start.HasValue && start.Value > now.AddDays(7) && !requestedCourses.Contains(course.Id))
                {
                    available.Add(courseDto);
                }
            }

            return available;
        }

        public void ShowRequestedCourses()
        {
            List<int> courseRequests = _studentsController.GetRequestedCourses(_studentId);
            List<int> requestedCourses = _enrollmentsController.GetCourseIds(courseRequests);
            _availableCourses = GetCoursesByIds(requestedCourses);
            _filtered = _availableCourses;
            _sorted = ApplySort(_filtered, _sortKey, _sortDir).ToList();
            ApplyFiltersSortAndPage();
        }
        private List<CourseDTO> GetCoursesByIds(List<int> courseIds)
        {
            List<CourseDTO> courses = new();
            foreach (int id in courseIds)
            {
                Course? course = _courseController.GetById(id);
                if (course != null)
                    courses.Add(new CourseDTO(course));
            }
            return courses;
        }

        public void ShowActiveCourse()
        {
            Student? student = _studentsController.GetStudentById(_studentId);
            if (student != null && student.ActiveCourseId != -1)
            {
                Course? course = _courseController.GetById(student.ActiveCourseId);
                if (course != null)
                    _availableCourses = new List<CourseDTO> { new CourseDTO(course) };
                else
                    _availableCourses = new List<CourseDTO>();
            }
            else
                _availableCourses = new List<CourseDTO>();
            _filtered = _availableCourses;
            _sorted = ApplySort(_filtered, _sortKey, _sortDir).ToList();
            ApplyFiltersSortAndPage();
        }

        public void SearchCourses()
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

        public void CoursesGridSorting(DataGridSortingEventArgs e)
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
        public void PrevPage()
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                ApplyFiltersSortAndPage(resetPage: false);
            }
        }
        public void NextPage()
        {
            var totalPages = Math.Max(1, (int)Math.Ceiling((double)_sorted.Count / _pageSize));
            if (_currentPage < totalPages)
            {
                _currentPage++;
                ApplyFiltersSortAndPage(resetPage: false);
            }
        }

        public void RequestForCourse()
        {
            if (CoursesGrid.SelectedItem is CourseDTO selectedCourse)
            {
                Student? student = _studentsController.GetStudentById(_studentId);
                if (student==null || student.ActiveCourseId!=-1)
                {
                    MessageBox.Show("You can only send enrollment request for a new course if you are not currently enrolled in any course.", "Enrollment Request Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int courseEnrollmentId=_enrollmentsController.Add(new CourseEnrollment(0,_studentId, selectedCourse.Id, DateTime.Now.ToString("yyyy-MM-dd"), RequestStatus.Pending));
                _studentsController.AddCourseRequest(_studentId, courseEnrollmentId);
                MessageBox.Show("Enrollment request submitted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                _availableCourses.RemoveAll(c => c.Id == selectedCourse.Id);
                ApplyFiltersSortAndPage(resetPage: true);
            }
            else
                MessageBox.Show("Please select a course to enroll in.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        public void CancelRequestForCourse()
        {
            if (CoursesGrid.SelectedItem is CourseDTO selectedCourse)
            {
                if(GetStartDateTime(selectedCourse) is DateTime start && start < DateTime.Now.AddDays(7))
                {
                    MessageBox.Show("You can only cancel enrollment request of course that start in more than 7 days.", "Cancellation Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                MessageBoxResult result = MessageBox.Show("Are you sure you want to cancel enrollment request for the selected course?", "Confirm Cancellation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    List<int> courseRequests = _studentsController.GetRequestedCourses(_studentId);
                    int courseRequestId=-1;
                    foreach (int requestId in courseRequests)
                    {
                        CourseEnrollment? courseRequest = _enrollmentsController.GetCourseEnrollmentById(requestId);
                        if (courseRequest!=null && courseRequest.TeachingActivityID == selectedCourse.Id)
                        {
                            courseRequestId = courseRequest.ID;
                            break;
                        }
                    }

                    _enrollmentsController.Delete(courseRequestId);
                    _studentsController.RemoveCourseRequest(_studentId, courseRequestId);
                    MessageBox.Show("Cancelled enrollment request successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    _availableCourses.Remove(_availableCourses.First(c => c.Id == selectedCourse.Id));
                    ApplyFiltersSortAndPage(resetPage: true);
                }
            }
            else
                MessageBox.Show("Please select a course request to cancel.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        public void UnenrollRequestCourse()
        {
            string professorId=string.Empty;
            Course? course = null;
            Student? student = _studentsController.GetStudentById(_studentId);
            if (student != null && student.ActiveCourseId != -1)
            {
                course = _courseController.GetById(student.ActiveCourseId);
                if (course != null)
                {
                    if(GetStartDateTime(new CourseDTO(course)) is DateTime start && start.AddDays(7) > DateTime.Now)
                    {
                        MessageBox.Show("You can onln send unenrollment request for course that is going on for more than 7 days.", "Unenrollment Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    professorId = course.ProfessorId;
                }
            }

            MessageSendWindow messageWindow = new MessageSendWindow(_studentId, "Unenrollment Request", "Please provide a reason for unenrolling from the course:", professorId, _messagesController, "professor");
            messageWindow.Show();
        }
    }
}