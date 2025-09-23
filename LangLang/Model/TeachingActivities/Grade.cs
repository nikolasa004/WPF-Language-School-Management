using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LangLang.Model.TeachingActivities
{
    public class Grade
    {
        public int Knowledge { get; set; }
        public int Activity { get; set; }

        public Grade() { }

        public Grade(int exam, int homework)
        {
            Knowledge = exam;
            Activity = homework;
        }

        public override string ToString()
        {
            return $"Knowledge: {Knowledge}, Activity: {Activity}";
        }
    }
}

