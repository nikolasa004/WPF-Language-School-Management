using LangLang.Storage.Serialization;
using System.Collections.Generic;
using System.Text.Json;

namespace LangLang.Model.TeachingActivities
{
    public class ComplimentedCourses:ISerializable
    {
        public List<int> Courses { get; set; }
        public ComplimentedCourses() { Courses = new List<int>(); }
        public ComplimentedCourses(List<int> courses) { this.Courses = courses; }
        public string ToJSON()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
        public void FromJSON(string value)
        {
            ComplimentedCourses? temp = JsonSerializer.Deserialize<ComplimentedCourses>(value);
            if (temp != null)
            {
                Courses = temp.Courses;
            }
        }
    }
}
