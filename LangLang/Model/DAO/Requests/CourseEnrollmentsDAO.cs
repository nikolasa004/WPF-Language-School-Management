using LangLang.Model.Requests;
using LangLang.Observer;
using LangLang.Storage;
using System.Collections.Generic;
using System.Xml.Linq;

namespace LangLang.Model.DAO.Requests
{
    class CourseEnrollmentsDAO
    {
        private readonly List<CourseEnrollment> _courseEnrollments;
        private readonly Storage<CourseEnrollment> _storage;
        public Subject CourseEnrollmentSubject;
        public CourseEnrollmentsDAO()
        {
            _storage = new Storage<CourseEnrollment>("course_enrollments.json");
            _courseEnrollments = _storage.Load();
            CourseEnrollmentSubject = new Subject();
        }
        public List<CourseEnrollment> GetAll() => _courseEnrollments;
        public void Add(CourseEnrollment e) => AddCourseEnrollment(e);
        public void Update(CourseEnrollment e) => UpdateCourseEnrollment(e);
        public void Remove(CourseEnrollment e){
            if (e == null) return;
            RemoveCourseEnrollment(e.ID);
        }
        public bool RemoveById(int id) => RemoveCourseEnrollment(id) != null;
        public void SaveChanges() => _storage.Save(_courseEnrollments);
        public void Load(){
            var fresh = _storage.Load();
            _courseEnrollments.Clear();
            _courseEnrollments.AddRange(fresh);
        }
        public int NextId(){
            return _courseEnrollments.Count == 0 ? 1 : _courseEnrollments[^1].ID + 1;
        }
        public CourseEnrollment AddCourseEnrollment(CourseEnrollment courseEnrollment){
            int nextId = _courseEnrollments.Count > 0 ? _courseEnrollments[^1].ID + 1 : 1;
            courseEnrollment.ID = nextId;
            _courseEnrollments.Add(courseEnrollment);
            _storage.Save(_courseEnrollments);
            CourseEnrollmentSubject.NotifyObservers();
            return courseEnrollment;
        }
        public CourseEnrollment? UpdateCourseEnrollment(CourseEnrollment courseEnrollment){
            CourseEnrollment? oldCourseEnrollment = GetCourseEnrollmentById(courseEnrollment.ID);
            if (oldCourseEnrollment is null) return null;
            oldCourseEnrollment.StudentID = courseEnrollment.StudentID;
            oldCourseEnrollment.TeachingActivityID = courseEnrollment.TeachingActivityID;
            oldCourseEnrollment.RequestDate = courseEnrollment.RequestDate;
            oldCourseEnrollment.Status = courseEnrollment.Status;
            _storage.Save(_courseEnrollments);
            CourseEnrollmentSubject.NotifyObservers();
            return oldCourseEnrollment;
        }
        public CourseEnrollment? RemoveCourseEnrollment(int id){
            CourseEnrollment? courseEnrollment = GetCourseEnrollmentById(id);
            if (courseEnrollment == null) return null;
            _courseEnrollments.Remove(courseEnrollment);
            _storage.Save(_courseEnrollments);
            CourseEnrollmentSubject.NotifyObservers();
            return courseEnrollment;
        }
        public void RemoveEnrollmentsByStudent(string studentId){
            _courseEnrollments.RemoveAll(ce => ce.StudentID == studentId);
            _storage.Save(_courseEnrollments);
            CourseEnrollmentSubject.NotifyObservers();
        }
        public CourseEnrollment? GetCourseEnrollmentById(int id){
            return _courseEnrollments.Find(ce => ce.ID == id);
        }
        public List<CourseEnrollment> GetAllCourseEnrollments(){
            return _courseEnrollments;
        }}}