using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LangLang.Model.TeachingActivities;

namespace LangLang.SQL
{
    public class LangLangContext : DbContext
    {
        public DbSet<PenaltyPoint> PenaltyPoints { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // SQLite fajl u root direktorijumu projekta
            options.UseSqlite("Data Source=langlang.db");
        }
    }
}
