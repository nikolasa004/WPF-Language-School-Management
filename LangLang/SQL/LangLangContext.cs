using Microsoft.EntityFrameworkCore;
using LangLang.Model.TeachingActivities;

namespace LangLang.SQL
{
    public class LangLangContext : DbContext
    {
        public DbSet<PenaltyPoint> PenaltyPoints { get; set; }
        public DbSet<ExamResult> ExamResults { get; set; }
        public DbSet<ExamTerm> ExamTerms { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var connectionString = "Data Source=langlang.db";
            options.UseSqlite(connectionString, x => x.CommandTimeout(60));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1 to many relationship between ExamTerm and ExamResult
            modelBuilder.Entity<ExamTerm>()
                .HasMany(e => e.ExamResults)
                .WithOne(r => r.ExamTerm)
                .HasForeignKey(r => r.ExamId)
                .OnDelete(DeleteBehavior.Cascade); 
        }
    }
}