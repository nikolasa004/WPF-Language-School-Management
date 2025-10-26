using Microsoft.EntityFrameworkCore;
using LangLang.Model.TeachingActivities;

namespace LangLang.SQL
{
    public class LangLangContext : DbContext
    {
        public DbSet<PenaltyPoint> PenaltyPoints { get; set; }
        public DbSet<ExamResult> ExamResults { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var connectionString = "Data Source=langlang.db";
            options.UseSqlite(connectionString, x => x.CommandTimeout(60));

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }
    }
}
