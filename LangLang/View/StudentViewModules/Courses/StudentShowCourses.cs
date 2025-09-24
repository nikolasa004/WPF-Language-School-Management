using LangLang.Controller.Requests;
using LangLang.Controller.TeachingActivities;
using LangLang.Controller.Users;
using LangLang.DTO;
using LangLang.Model.TeachingActivities;
using LangLang.Model.Users;
using LangLang.Utils;
using System;
using System.Collections.Generic;

namespace LangLang.View.StudentViewModules.Courses
{
    internal class StudentShowCourses
    {
        private readonly StudentsController _studentsController;
        private readonly ProfessorsController _professorsController;
        private readonly CourseController _courseController;
        private readonly CourseEnrollmentsController _enrollmentsController;
        private readonly string _studentId;
        private List<CourseDTO> _availableCourses;
        private List<CourseDTO> _filtered;
        
        public StudentShowCourses(StudentsController studentsController, ProfessorsController professorsController, CourseController courseController, CourseEnrollmentsController enrollmentsController, string studentId, List<CourseDTO> availableCourses, List<CourseDTO> filtered)
        {
            _studentsController = studentsController;
            _professorsController = professorsController;
            _courseController = courseController;
            _enrollmentsController = enrollmentsController;
            _studentId = studentId;
            _availableCourses = availableCourses;
            _filtered = filtered;
        }

        public RateWindow? CheckIfActiveCourseFinished()
        {
            Student? student = _studentsController.GetStudentById(_studentId)!;
            if (student.ActiveCourseId != -1)
            {
                Course? course = _courseController.GetById(student.ActiveCourseId)!;
                DateTime? start = GUIHelper.GetCourseDate(new CourseDTO(course));
                if (start.HasValue && start.Value.AddDays(course.DurationWeeks * 7) < DateTime.Now)
                {
                    RateWindow rateWindow = new RateWindow(course.ProfessorId, "professor", _professorsController);
                    _studentsController.FinishActiveCourse(_studentId, course.Language, course.Level);
                    return rateWindow;
                }
            }
            return null;
        }
        public void ShowAllCourses()
        {
            List<Course> allCourses = _courseController.GetAll();
            _availableCourses.Clear();
            _availableCourses.AddRange(GetAvailableCourses(allCourses));
            _filtered.Clear();
            _filtered.AddRange(_availableCourses);
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
                var start =GUIHelper.GetCourseDate(courseDto);
                List<int> courseRequests = _studentsController.GetRequestedCourses(_studentId);
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
            _availableCourses.Clear();
            _availableCourses.AddRange(GetCoursesByIds(requestedCourses));
            _filtered.Clear();
            _filtered.AddRange(_availableCourses);
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
            Student? student = _studentsController.GetStudentById(_studentId)!;
            _availableCourses.Clear();
            if (student.ActiveCourseId != -1)
            {
                Course? course = _courseController.GetById(student.ActiveCourseId)!;
                _availableCourses.Add(new CourseDTO(course));
            }
            _filtered.Clear();
            _filtered.AddRange(_availableCourses);
        }
        public void ShowFinishedCourses()
        {
            StudentFinishedLevelsWindow studentFinishedLevelsWindow = new StudentFinishedLevelsWindow(_studentsController, _studentId);
            studentFinishedLevelsWindow.Show();
        }
    }
}