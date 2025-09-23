using LangLang.Storage;
using System.Collections.Generic;
using LangLang.Observer;
using LangLang.Model.Users;
using LangLang.Model.Enums;
using System;
using System.Globalization;

namespace LangLang.Model.DAO.Users
{
    class StudentsDAO
    {
        private readonly List<Student> _students;
        private readonly Storage<Student> _storage;

        public Subject StudentSubject;

        public StudentsDAO()
        {
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
            oldStudent.LastPenaltyProcessedAt = student.LastPenaltyProcessedAt; 
            oldStudent.RequestedExamTerms = student.RequestedExamTerms;
            oldStudent.ActiveExamTermId = student.ActiveExamTermId;
            oldStudent.FinishedExams = student.FinishedExams;
            oldStudent.IsExamApplicationLocked = student.IsExamApplicationLocked;


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

        private static DateTime MonthStart(DateTime d) => new DateTime(d.Year, d.Month, 1);

        private static DateTime ParseMonthString(string? s, DateTime fallbackNow)
        {
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

                s.LastPenaltyProcessedAt = monthStamp;
                changed++;
            }

            if (changed > 0)
            {
                _storage.Save(_students);
                StudentSubject.NotifyObservers();
            }

            return changed;
        }


        public List<int> GetRequestedExamTerms(string email)
        {
            var s = GetStudentById(email);
            return s?.RequestedExamTerms ?? new List<int>();
        }

        public int GetActiveExamTermId(string email)
        {
            var s = GetStudentById(email);
            return s?.ActiveExamTermId ?? -1;
        }

        public List<Tuple<Language, LanguageLevel>> GetFinishedExams(string email)
        {
            var s = GetStudentById(email);
            return s?.FinishedExams ?? new List<Tuple<Language, LanguageLevel>>();
        }

        public void AddExamRequest(string studentEmail, int examTermId)
        {
            var s = GetStudentById(studentEmail);
            if (s == null) return;

            if (!s.RequestedExamTerms.Contains(examTermId))
                s.RequestedExamTerms.Add(examTermId);

            if (s.ActiveExamTermId == -1)
                s.ActiveExamTermId = examTermId;

            _storage.Save(_students);
            StudentSubject.NotifyObservers();
        }

        public void RemoveExamRequest(string studentEmail, int examTermId)
        {
            var s = GetStudentById(studentEmail);
            if (s == null) return;

            s.RequestedExamTerms.Remove(examTermId);
            if (s.ActiveExamTermId == examTermId)
                s.ActiveExamTermId = -1;

            _storage.Save(_students);
            StudentSubject.NotifyObservers();
        }

        public void SetActiveExamTerm(string studentEmail, int examTermId)
        {
            var s = GetStudentById(studentEmail);
            if (s == null) return;

            s.ActiveExamTermId = examTermId;
            if (!s.RequestedExamTerms.Contains(examTermId))
                s.RequestedExamTerms.Add(examTermId);

            _storage.Save(_students);
            StudentSubject.NotifyObservers();
        }

        public void ClearActiveExamTerm(string studentEmail)
        {
            var s = GetStudentById(studentEmail);
            if (s == null) return;

            s.ActiveExamTermId = -1;
            _storage.Save(_students);
            StudentSubject.NotifyObservers();
        }

        public void FinishActiveExam(string studentEmail, Language language, LanguageLevel level)
        {
            var s = GetStudentById(studentEmail);
            if (s == null) return;

            s.FinishedExams.Add(new Tuple<Language, LanguageLevel>(language, level));
            s.ActiveExamTermId = -1;

            _storage.Save(_students);
            StudentSubject.NotifyObservers();
        }

        public void LockExamApplications(string studentEmail)
        {
            var s = GetStudentById(studentEmail);
            if (s == null) return;

            s.IsExamApplicationLocked = true;
            _storage.Save(_students);
            StudentSubject.NotifyObservers();
        }

        public void UnlockExamApplications(string studentEmail)
        {
            var s = GetStudentById(studentEmail);
            if (s == null) return;

            s.IsExamApplicationLocked = false;
            _storage.Save(_students);
            StudentSubject.NotifyObservers();
        }
        public void RecordExamDecision(string studentEmail, Language language, LanguageLevel level, bool passed)
        {
            // 1) Ažuriraj stanje ispita
            if (passed)
            {
                // koristi već postojeću metodu da doda u FinishedExams i očisti ActiveExamTermId
                FinishActiveExam(studentEmail, language, level);
            }
            else
            {
                // nije položio — ukloni aktivni termin, može opet da se prijavi
                ClearActiveExamTerm(studentEmail);
            }

            // 2) Otključaj prijave ispita
            var s = GetStudentById(studentEmail);
            if (s == null) return;

            s.IsExamApplicationLocked = false;
            _storage.Save(_students);
            StudentSubject.NotifyObservers();
        }


    }
}
