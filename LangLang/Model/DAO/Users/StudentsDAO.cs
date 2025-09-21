using LangLang.Storage;
using System.Collections.Generic;
using LangLang.Observer;
using LangLang.Model.Users;
using LangLang.Model.Enums;
using System;
using System.Globalization;   // ⬅️ DODATO (zbog parsiranja "yyyy-MM-dd")

namespace LangLang.Model.DAO.Users
{
    class StudentsDAO
    {
        private readonly List<Student> _students;
        private readonly Storage<Student> _storage;

        public Subject StudentSubject;

        public StudentsDAO()
        {
            // ostaje kao u originalu
            _storage = new Storage<Student>("students.json");
            _students = _storage.Load();
            StudentSubject = new Subject();
        }

        public Student AddStudent(Student student)
        {
            _students.Add(student);
            _storage.Save(_students);
            StudentSubject.NotifyObservers();
            return student;
        }

        public Student? UpdateStudent(Student student)
        {
            Student? oldStudent = GetStudentById(student.Email);
            if (oldStudent is null) return null;

            oldStudent.FirstName = student.FirstName;
            oldStudent.LastName = student.LastName;
            oldStudent.Gender = student.Gender;
            oldStudent.DateOfBirth = student.DateOfBirth;
            oldStudent.PhoneNumber = student.PhoneNumber;
            oldStudent.Password = student.Password;
            oldStudent.EducationLevel = student.EducationLevel;
            oldStudent.DateOfRegistration = student.DateOfRegistration;
            oldStudent.PenaltyPoints = student.PenaltyPoints;
            oldStudent.RequestedCourses = student.RequestedCourses;
            oldStudent.ActiveCourseId = student.ActiveCourseId;
            oldStudent.FinishedCourses = student.FinishedCourses;
            oldStudent.LastPenaltyProcessedAt = student.LastPenaltyProcessedAt; // ⬅️ DODATO

            _storage.Save(_students);
            StudentSubject.NotifyObservers();
            return oldStudent;
        }

        public Student? RemoveStudent(string email)
        {
            Student? student = GetStudentById(email);
            if (student == null) return null;

            _students.Remove(student);
            _storage.Save(_students);
            StudentSubject.NotifyObservers();
            return student;
        }

        public Student? GetStudentById(string email) => _students.Find(s => s.Email == email);
        public List<Student> GetAllStudents() => _students;

        public List<int> GetRequestedCourses(string email)
        {
            Student? student = GetStudentById(email);
            if (student == null) return new List<int>();
            return student.RequestedCourses;
        }

        public List<Tuple<Language, LanguageLevel>> GetFinishedCourses(string email)
        {
            Student? student = GetStudentById(email);
            if (student == null) return new List<Tuple<Language, LanguageLevel>>();
            return student.FinishedCourses;
        }

        public void FinishActiveCourse(string studentEmail, Language language, LanguageLevel level)
        {
            Student? student = GetStudentById(studentEmail);
            if (student != null && student.ActiveCourseId != -1)
            {
                student.FinishedCourses.Add(new Tuple<Language, LanguageLevel>(language, level));
                student.ActiveCourseId = -1;
                _storage.Save(_students);
                StudentSubject.NotifyObservers();
            }
        }

        // ------------------------------
        //  PENALTY DATE / MONTHLY DECAY
        // ------------------------------

        private static DateTime MonthStart(DateTime d) => new DateTime(d.Year, d.Month, 1);

        private static DateTime ParseMonthString(string? s, DateTime fallbackNow)
        {
            // očekujemo "yyyy-MM-dd"; normalizujemo na 1. u mesecu
            if (!string.IsNullOrWhiteSpace(s) &&
                DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                       DateTimeStyles.None, out var dt))
            {
                return MonthStart(dt);
            }
            return MonthStart(fallbackNow);
        }

        private static int MonthsPassed(string? lastProcessedAt, DateTime now)
        {
            var a = ParseMonthString(lastProcessedAt, now);
            var b = MonthStart(now);
            return Math.Max(0, (b.Year - a.Year) * 12 + (b.Month - a.Month));
        }

        /// <summary>
        /// Svim studentima smanjuje PenaltyPoints za broj pređenih meseci (max do 0)
        /// i postavlja LastPenaltyProcessedAt na tekući mesec (yyyy-MM-01).
        /// Vraća broj studenata kod kojih je bilo izmene.
        /// </summary>
        public int ApplyMonthlyPenaltyDecayForAll(DateTime now)
        {
            int changed = 0;
            string monthStamp = MonthStart(now).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            foreach (var s in _students)
            {
                int months = MonthsPassed(s.LastPenaltyProcessedAt, now);
                if (months <= 0) continue;

                if (s.PenaltyPoints > 0)
                {
                    int dec = Math.Min(s.PenaltyPoints, months);
                    s.PenaltyPoints -= dec;
                }

                s.LastPenaltyProcessedAt = monthStamp; // zaključamo na tekući mesec
                changed++;
            }

            if (changed > 0)
            {
                _storage.Save(_students);
                StudentSubject.NotifyObservers();
            }

            return changed;
        }
    }
}
