using LangLang.Storage;
using LangLang.Model.TeachingActivities;
using System.Collections.Generic;

namespace LangLang.Controller.TeachingActivities
{
    internal class ComplimentedCoursesController
    {
        private ComplimentedCourses complimentedCourses;
        private Storage<ComplimentedCourses> storage;
        public ComplimentedCoursesController()
        {
            storage = new Storage<ComplimentedCourses>("complimentedCourses.json");
            List<ComplimentedCourses> temp = storage.Load();
            if (temp.Count > 0)
                complimentedCourses = temp[0];
            else
                complimentedCourses = new ComplimentedCourses();
        }
        public List<int> GetAll()
        {
            return complimentedCourses.Courses;
        }
        public void Add(int courseId)
        {
            if (!complimentedCourses.Courses.Contains(courseId))
            {
                complimentedCourses.Courses.Add(courseId);
                storage.Save(new List<ComplimentedCourses> { complimentedCourses });
            }
        }
        public void Remove(int courseId)
        {
            if (complimentedCourses.Courses.Contains(courseId))
            {
                complimentedCourses.Courses.Remove(courseId);
                storage.Save(new List<ComplimentedCourses> { complimentedCourses });
            }
        }
    }
}
