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

        public int Add(CourseEnrollment courseEnrollment)
        {
            return _courseEnrollments.AddCourseEnrollment(courseEnrollment).ID;
        }
        public CourseEnrollment? Update(CourseEnrollment courseEnrollment)
        {
            return _courseEnrollments.UpdateCourseEnrollment(courseEnrollment);
        }
        public void Delete(int courseEnrollmentId)
        {
            _courseEnrollments.RemoveCourseEnrollment(courseEnrollmentId);
        }

        public CourseEnrollment? GetCourseEnrollmentById(int id)
        {
            return _courseEnrollments.GetCourseEnrollmentById(id);
        }
        public List<int> GetCourseIds(List<int> courseEnrollmentIds)
        {
            List<int> courseIds = new List<int>();
            foreach (int id in courseEnrollmentIds)
            {
                CourseEnrollment? courseEnrollment = _courseEnrollments.GetCourseEnrollmentById(id);
                if (courseEnrollment != null)
                {
                    courseIds.Add(courseEnrollment.TeachingActivityID);
                }
            }
            return courseIds;
        }

        public void Subscribe(IObserver observer)
        {
            _courseEnrollments.CourseEnrollmentSubject.Subscribe(observer);
        }
    }
}
