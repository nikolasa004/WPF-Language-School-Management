using LangLang.Controller.Messages;
using LangLang.Controller.TeachingActivities;
using LangLang.Controller.Users;
using LangLang.DTO;
using LangLang.Model.Messages;
using LangLang.Model.TeachingActivities;
using LangLang.Model.Users;
using LangLang.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LangLang.View.PrincipalViewModules
{
    internal class PrincipalCourseView
    {
        private readonly CourseController _courseController;
        private readonly ComplimentedCoursesController _complimentedCoursesController;

        private readonly ObservableCollection<CourseDTO> _coursesObservable = new();
        private DataGrid coursesGrid;

        public PrincipalCourseView(DataGrid coursesGrid)
        {
            _courseController = new CourseController();
            _complimentedCoursesController = new ComplimentedCoursesController();
            this.coursesGrid = coursesGrid;

            coursesGrid.ItemsSource = _coursesObservable;
            LoadCourses();
        }

        private void LoadCourses()
        {
            _coursesObservable.Clear();
            List<CourseDTO> allCourses = _courseController.GetAll().Select(e => new CourseDTO(e)).ToList();
            ComplimentedCoursesController complimentedCoursesController = new ComplimentedCoursesController();
            List<CourseDTO> finishedCourses= new List<CourseDTO>();
            foreach (var course in allCourses)
            {
                DateTime? date = GUIHelper.GetCourseDate(course);
                if (date.HasValue)
                {
                    DateTime fullDate = date.Value;
                    if (fullDate.AddDays(course.DurationWeeks * 7) < DateTime.Now && fullDate.AddDays((course.DurationWeeks + 1) * 7) > DateTime.Now)
                    {
                        finishedCourses.Add(course);
                    }
                }
            }
            CheckAlreadyComplimented(finishedCourses);
        }
        private void CheckAlreadyComplimented(List<CourseDTO> finishedCourses)
        {
            var complimented = _complimentedCoursesController.GetAll().ToList();
            foreach (var courseId in complimented)
            {
                if (!finishedCourses.Any(c => c.Id == courseId))
                    _complimentedCoursesController.Remove(courseId);
            }
            var complimentedSet = new HashSet<int>(_complimentedCoursesController.GetAll());
            foreach (var course in finishedCourses)
            {
                if (!complimentedSet.Contains(course.Id))
                    _coursesObservable.Add(course);
            }
        }
        public void SendCompliments(int courseId, string type)
        {
            _complimentedCoursesController.Add(courseId);
            MessagesController messagesController = new MessagesController();
            StudentsController studentsController = new StudentsController();
            Course course = _courseController.GetById(courseId)!;
            Dictionary<string, Grade> grades = course.Grades;

            var topStudents = grades.OrderByDescending(g => type == "knowledge" ? g.Value.Knowledge : g.Value.Activity).ThenBy(g => { var student = studentsController.GetStudentById(g.Key); return student?.PenaltyPoints ?? int.MaxValue; }).Select(g => g.Key).ToList();

            foreach (var studentId in topStudents)
            {
                var student = studentsController.GetStudentById(studentId);
                messagesController.StudentAdd(MakeMessage(student!, course, type));
            }
        }
        public Message MakeMessage(Student student, Course course, string type)
        {
            Message message = new Message
            {
                RecipientId = student.Email,
                SenderId = "principal@gmail.com",
                Title = "Thank you for your effort!",
                Body = $"Dear {student.FirstName},\n\n" +
                               $"We would like to express our sincere appreciation for your outstanding {type} in the recently concluded course {course.Language} {course.Level}.\n\n" +
                               $"Your dedication and hard work have not gone unnoticed, and we are truly grateful to have students like you in our institution.\n\n" +
                               $"Keep up the excellent work!\n\n" +
                               $"Best regards,\n" +
                               $"The Principal"
            };
            return message;
        }
        public void RefreshCourses()
        {
            LoadCourses();
        }   
    }
}
