using System;
using System.Globalization;
using System.Linq;

namespace LangLang.Model.DAO.Users.DAOHelpers
{
    class StudentsPenaltyDAO
    {
        private StudentsDAO _studentsDAO;
        public StudentsPenaltyDAO(StudentsDAO studentsDAO)
        {
            _studentsDAO = studentsDAO;
        }

        private static DateTime MonthStart(DateTime d) => new DateTime(d.Year, d.Month, 1);
        private static DateTime ParseMonthString(string? s, DateTime fallbackNow)
        {
            if (!string.IsNullOrWhiteSpace(s) && DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return MonthStart(dt);
            return MonthStart(fallbackNow);
        }
        private static int MonthsPassed(string? lastProcessedAt, DateTime now)
        {
            var a = ParseMonthString(lastProcessedAt, now);
            var b = MonthStart(now);
            return Math.Max(0, (b.Year - a.Year) * 12 + (b.Month - a.Month));
        }
        public int ApplyMonthlyPenaltyDecayForAll(DateTime now)
        {
            int changed = 0;
            string monthStamp = MonthStart(now).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            foreach (var s in _studentsDAO.GetAllStudents())
            {
                int months = MonthsPassed(s.LastPenaltyProcessedAt, now);
                if (months <= 0) continue;
                if (s.PenaltyPoints > 0)
                {
                    int dec = Math.Min(s.PenaltyPoints, months);
                    s.PenaltyPoints -= dec;
                }
                s.LastPenaltyProcessedAt = monthStamp;
                changed++;
            }

            if (changed > 0)
                _studentsDAO.SaveChanges();

            return changed;
        }
        public void AddPenaltyPoint(string email)
        {
            var student = _studentsDAO.GetStudentById(email);
            if (student == null) return;

            student.PenaltyPoints += 1;
            student.LastPenaltyProcessedAt = DateTime.Now.ToString("yyyy-MM-dd");
            _studentsDAO.SaveChanges();
        }
    }
}