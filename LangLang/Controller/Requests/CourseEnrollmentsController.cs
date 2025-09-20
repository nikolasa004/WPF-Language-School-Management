using LangLang.Model.DAO.Requests;
using LangLang.Model.Requests;
using LangLang.Observer;
using System.Collections.Generic;

namespace LangLang.Controller.Requests
{
    public class CourseEnrollmentsController
    {
        private readonly CourseEnrollmentsDAO _courseEnrollments;

        public CourseEnrollmentsController()
        {
            _courseEnrollments = new CourseEnrollmentsDAO();
        }

        public List<CourseEnrollment> GetAllCourseEnrollments()
        {
            return _courseEnrollments.GetAllCourseEnrollments();
        }

        public void Add(CourseEnrollment courseEnrollment)
        {
            _courseEnrollments.AddCourseEnrollment(courseEnrollment);
        }
        public CourseEnrollment? Update(CourseEnrollment courseEnrollment)
        {
            return _courseEnrollments.UpdateCourseEnrollment(courseEnrollment);
        }
        public void Delete(int courseEnrollmentId)
        {
            _courseEnrollments.RemoveCourseEnrollment(courseEnrollmentId);
        }

        public CourseEnrollment? GetStudentById(int id)
        {
            return _courseEnrollments.GetCourseEnrollmentById(id);
        }

        public void Subscribe(IObserver observer)
        {
            _courseEnrollments.CourseEnrollmentSubject.Subscribe(observer);
        }
    }
}
