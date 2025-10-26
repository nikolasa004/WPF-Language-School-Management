using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using LangLang.Model.TeachingActivities;

namespace LangLang.SQL.Repositories
{
    public class PenaltyPointRepository
    {
        public void Add(PenaltyPoint penaltyPoint)
        {
            using (var context = new LangLangContext())
            {
                context.Database.EnsureCreated();
                context.PenaltyPoints.Add(penaltyPoint);
                context.SaveChanges();
            }
        }

        public List<PenaltyPoint> GetAll()
        {
            using (var context = new LangLangContext())
            {
                return context.PenaltyPoints.ToList();
            }
        }
    }
}
