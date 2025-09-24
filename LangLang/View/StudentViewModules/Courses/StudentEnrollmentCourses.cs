using LangLang.Controller.Users;
using LangLang.DTO;
using LangLang.Model.Enums;
using LangLang.Model.Requests;
using LangLang.Model.TeachingActivities;
using LangLang.Model.Users;
using LangLang.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using LangLang.Controller.TeachingActivities;
using LangLang.Controller.Requests;
using LangLang.Controller.Messages;
using System.Windows.Controls;

namespace LangLang.View.StudentViewModules.Courses
{
    class StudentEnrollmentCourses
    {
        private readonly StudentsController _studentsController;
        private readonly CourseController _courseController;
        private readonly CourseEnrollmentsController _enrollmentsController;
        private readonly MessagesController _messagesController;

        private readonly string _studentId;
        private List<CourseDTO> _availableCourses;
        private DataGrid CoursesGrid;

        public StudentEnrollmentCourses(StudentsController studentsController, CourseController courseController, CourseEnrollmentsController enrollmentsController, MessagesController messagesController, string studentId, List<CourseDTO> availableCourses, DataGrid coursesGrid)
        {
            _studentsController = studentsController;
            _courseController = courseController;
            _enrollmentsController = enrollmentsController;
            _messagesController = messagesController;
            _studentId = studentId;
            _availableCourses = availableCourses;
            CoursesGrid = coursesGrid;
        }

        public bool RequestForCourse()
        {
            if (CoursesGrid.SelectedItem is CourseDTO selectedCourse)
            {
                Student? student = _studentsController.GetStudentById(_studentId);
                if (student == null || student.ActiveCourseId != -1)
                {
                    MessageBox.Show("You can only send enrollment request for a new course if you are not currently enrolled in any course.", "Enrollment Request Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                int courseEnrollmentId = _enrollmentsController.Add(new CourseEnrollment(0, _studentId, selectedCourse.Id, DateTime.Now.ToString("yyyy-MM-dd"), RequestStatus.Pending));
                _studentsController.AddCourseRequest(_studentId, courseEnrollmentId);
                MessageBox.Show("Enrollment request submitted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                _availableCourses.RemoveAll(c => c.Id == selectedCourse.Id);
                return true;
            }
            else
                MessageBox.Show("Please select a course to enroll in.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        public bool CancelRequestForCourse()
        {
            if (CoursesGrid.SelectedItem is CourseDTO selectedCourse)
            {
                if (GUIHelper.GetCourseDate(selectedCourse) is DateTime start && start < DateTime.Now.AddDays(7))
                {
                    MessageBox.Show("You can only cancel enrollment request of course that start in more than 7 days.", "Cancellation Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                MessageBoxResult result = MessageBox.Show("Are you sure you want to cancel enrollment request for the selected course?", "Confirm Cancellation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    int courseRequestId=GetRequestFromCourse(selectedCourse.Id);

                    _enrollmentsController.Delete(courseRequestId);
                    _studentsController.RemoveCourseRequest(_studentId, courseRequestId);
                    MessageBox.Show("Cancelled enrollment request successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    _availableCourses.Remove(_availableCourses.First(c => c.Id == selectedCourse.Id));
                    return true;
                }
            }
            else
                MessageBox.Show("Please select a course request to cancel.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        public int GetRequestFromCourse(int selectedCourseId)
        {
            List<int> courseRequests = _studentsController.GetRequestedCourses(_studentId);
            int courseRequestId = -1;
            foreach (int requestId in courseRequests)
            {
                CourseEnrollment? courseRequest = _enrollmentsController.GetCourseEnrollmentById(requestId)!;
                if (courseRequest.TeachingActivityID == selectedCourseId)
                {
                    courseRequestId = courseRequest.ID;
                    break;
                }
            }

            return courseRequestId;
        }
        public void UnenrollRequestCourse()
        {
            string professorId = string.Empty;
            Course? course = null;
            Student? student = _studentsController.GetStudentById(_studentId)!;
            if (student.ActiveCourseId != -1)
            {
                course = _courseController.GetById(student.ActiveCourseId)!;
                if (GUIHelper.GetCourseDate(new CourseDTO(course)) is DateTime start && start.AddDays(7) > DateTime.Now)
                {
                    MessageBox.Show("You can only send unenrollment request for course that is going on for more than 7 days.", "Unenrollment Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                professorId = course.ProfessorId;
            }

            MessageSendWindow messageWindow = new MessageSendWindow(_studentId, "Unenrollment Request", "Please provide a reason for unenrolling from the course:", professorId, _messagesController, "professor");
            messageWindow.Show();
        }
    }
}
