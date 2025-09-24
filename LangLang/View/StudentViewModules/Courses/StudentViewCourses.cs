using LangLang.Controller.Messages;
using LangLang.Controller.Requests;
using LangLang.Controller.TeachingActivities;
using LangLang.Controller.Users;
using LangLang.DTO;
using LangLang.Model.Enums;
using LangLang.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

namespace LangLang.View.StudentViewModules.Courses
{
    internal class StudentViewCourses
    {
        private readonly ProfessorsController _professorsController;

        private List<CourseDTO> _availableCourses;
        private List<CourseDTO> _filtered;   // after filters
        private List<CourseDTO> _sorted;     // after sorting
        private readonly ObservableCollection<CourseDTO> _page = new();

        private int _currentPage = 1;
        private int _pageSize = 10;
        private string _sortKey = "Date";
        private ListSortDirection _sortDir = ListSortDirection.Ascending;

        private StudentShowCourses studentShowCourses;
        private StudentEnrollmentCourses studentEnrollmentCourses;
        private StudentSortCourses studentSortCourses;

        private string _studentId;

        public StudentViewCourses(CourseController courseController, CourseEnrollmentsController courseEnrollmentsController, ComboBox courseLanguageCombo, ComboBox courseLevelCombo, ComboBox courseOnlineCombo, TextBox courseWeeksBox, DatePicker courseFromDate, DatePicker courseToDate, DataGrid coursesGrid, string studentId, StudentsController studentsController, MessagesController messagesController)
        {
            _professorsController = new ProfessorsController();
            _studentId = studentId;
            if (courseLanguageCombo.Items.Count == 0)
                courseLanguageCombo.ItemsSource = Enum.GetValues(typeof(Language));
            if (courseLevelCombo.Items.Count == 0)
                courseLevelCombo.ItemsSource = Enum.GetValues(typeof(LanguageLevel));
            courseLanguageCombo.SelectedIndex = -1;
            courseLevelCombo.SelectedIndex = -1;

            _availableCourses = new List<CourseDTO>();
            _filtered = new List<CourseDTO>();
            _sorted = new List<CourseDTO>();
            coursesGrid.ItemsSource = _page;

            studentShowCourses= new StudentShowCourses(studentsController, _professorsController, courseController, courseEnrollmentsController, _studentId, _availableCourses, _filtered);
            studentEnrollmentCourses = new StudentEnrollmentCourses(studentsController, courseController, courseEnrollmentsController, messagesController, _studentId, _availableCourses, coursesGrid);
            studentSortCourses = new StudentSortCourses(courseController, _availableCourses, _filtered, _sorted, courseLanguageCombo, courseLevelCombo, courseOnlineCombo, courseWeeksBox, courseFromDate, courseToDate, coursesGrid, _page, _pageSize, _currentPage, _sortKey, _sortDir);
        }

        public RateWindow? CheckIfActiveCourseFinished()
        {
            return studentShowCourses.CheckIfActiveCourseFinished();
        }
        public void ShowAllCourses()
        {
            studentShowCourses.ShowAllCourses();
            _sorted = GUIHelper.SortCourses(_filtered, _sortKey, _sortDir).ToList();
            ApplyFiltersSortAndPage();
        }
        public void ShowRequestedCourses()
        {
            studentShowCourses.ShowRequestedCourses();
            _sorted = GUIHelper.SortCourses(_filtered, _sortKey, _sortDir).ToList();
            ApplyFiltersSortAndPage();
        }
        public void ShowActiveCourse()
        {
            studentShowCourses.ShowActiveCourse();
            _sorted = GUIHelper.SortCourses(_filtered, _sortKey, _sortDir).ToList();
            ApplyFiltersSortAndPage();
        }
        public void ShowFinishedCourses()
        {
            studentShowCourses.ShowFinishedCourses();
        }
        public void RequestForCourse()
        {
            if (studentEnrollmentCourses.RequestForCourse())
                ApplyFiltersSortAndPage(resetPage: true);
        }
        public void CancelRequestForCourse()
        {
            if (studentEnrollmentCourses.CancelRequestForCourse())
                ApplyFiltersSortAndPage(resetPage: true);
        }
        public void UnenrollRequestCourse()
        {
            studentEnrollmentCourses.UnenrollRequestCourse();
        }

        public void ApplyFiltersSortAndPage(bool resetPage = true)
        {
            studentSortCourses.ApplyFiltersSortAndPage(resetPage);
        }
        public void CoursesGridSorting(DataGridSortingEventArgs e)
        {
            studentSortCourses.CoursesGridSorting(e);
        }
        public void PageSizeChanged(object sender)
        {
            studentSortCourses.PageSizeChanged(sender);
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
    }
}