using LangLang.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LangLang.Model.DAO.Users.DAOHelpers
{
    class StudentsExamDAO
    {
        private StudentsDAO _studentsDAO;
        public StudentsExamDAO(StudentsDAO studentsDAO)
        {
            _studentsDAO = studentsDAO;
        }

        public List<int> GetRequestedExamTerms(string email)
        {
            var s = _studentsDAO.GetStudentById(email);
            return s?.RequestedExamTerms ?? new List<int>();
        }
        public int GetActiveExamTermId(string email)
        {
            var s =_studentsDAO.GetStudentById(email);
            return s?.ActiveExamTermId ?? -1;
        }
        public List<Tuple<Language, LanguageLevel>> GetFinishedExams(string email)
        {
            var s = _studentsDAO.GetStudentById(email);
            return s?.FinishedExams ?? new List<Tuple<Language, LanguageLevel>>();
        }
        public void AddExamRequest(string studentEmail, int examTermId)
        {
            var s =_studentsDAO.GetStudentById(studentEmail);
            if (s == null) return;

            if (!s.RequestedExamTerms.Contains(examTermId))
                s.RequestedExamTerms.Add(examTermId);
            if (s.ActiveExamTermId == -1)
                s.ActiveExamTermId = examTermId;
            _studentsDAO.SaveChanges();
        }
        public void RemoveExamRequest(string studentEmail, int examTermId)
        {
            var s = _studentsDAO.GetStudentById(studentEmail);
            if (s == null) return;

            s.RequestedExamTerms.Remove(examTermId);
            if (s.ActiveExamTermId == examTermId)
                s.ActiveExamTermId = -1;
            _studentsDAO.SaveChanges();
        }
        public void SetActiveExamTerm(string studentEmail, int examTermId)
        {
            var s = _studentsDAO.GetStudentById(studentEmail);
            if (s == null) return;

            s.ActiveExamTermId = examTermId;
            if (!s.RequestedExamTerms.Contains(examTermId))
                s.RequestedExamTerms.Add(examTermId);
            _studentsDAO.SaveChanges();
        }
        public void ClearActiveExamTerm(string studentEmail)
        {
            var s = _studentsDAO.GetStudentById(studentEmail);
            if (s == null) return;

            s.ActiveExamTermId = -1;
            _studentsDAO.SaveChanges();
        }
        public void FinishActiveExam(string studentEmail, Language language, LanguageLevel level)
        {
            var s = _studentsDAO.GetStudentById(studentEmail);
            if (s == null) return;

            s.FinishedExams.Add(new Tuple<Language, LanguageLevel>(language, level));
            s.ActiveExamTermId = -1;
            _studentsDAO.SaveChanges();
        }
        public void LockExamApplications(string studentEmail)
        {
            var s = _studentsDAO.GetStudentById(studentEmail);
            if (s == null) return;

            s.IsExamApplicationLocked = true;
            _studentsDAO.SaveChanges();
        }
        public void UnlockExamApplications(string studentEmail)
        {
            var s = _studentsDAO.GetStudentById(studentEmail);
            if (s == null) return;

            s.IsExamApplicationLocked = false;
            _studentsDAO.SaveChanges();
        }
        public void RecordExamDecision(string studentEmail, Language language, LanguageLevel level, bool passed)
        {
            if (passed)
                FinishActiveExam(studentEmail, language, level);
            else
                ClearActiveExamTerm(studentEmail);

            var s = _studentsDAO.GetStudentById(studentEmail);
            if (s == null) return;

            s.IsExamApplicationLocked = false;
            _studentsDAO.SaveChanges();
        }
    }
}