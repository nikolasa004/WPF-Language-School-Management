using LangLang.Model.DAO.Requests;
using LangLang.Model.Requests;
using LangLang.Observer;
using System;
using System.Collections.Generic;

// ⬇️ dodaj ovo
using LangLang.Controller.TeachingActivities;
using LangLang.Model.TeachingActivities;

namespace LangLang.Controller.Requests
{
    public class ExamEnrollmentsController
    {
        private readonly ExamEnrollmentsDAO _examEnrollments;
        private readonly ExamTermsController _examTerms;   // ⬅️ singular

        public ExamEnrollmentsController()
        {
            _examEnrollments = new ExamEnrollmentsDAO();
            _examTerms = new ExamTermsController();
        }

        public List<ExamEnrollment> GetAllExamEnrollments()
            => _examEnrollments.GetAllExamEnrollments();

        public int? Add(ExamEnrollment examEnrollment)
        {
            var examTerm = _examTerms.GetById(examEnrollment.TeachingActivityID); // ⬅️ ova metoda sada postoji
            if (examTerm == null) return null;

            // Ako je termin popunjen, ne dozvoli prijavu
            int alreadyRegistered = _examEnrollments.GetByExamTerm(examTerm.Id).Count;    // ⬅️ ova metoda sada postoji
            if (alreadyRegistered >= examTerm.MaxAttendees) return null;

            return _examEnrollments.AddExamEnrollment(examEnrollment).ID;
        }

        public bool CancelEnrollment(int enrollmentId)
        {
            var enrollment = _examEnrollments.GetExamEnrollmentById(enrollmentId);
            if (enrollment == null) return false;

            var examTerm = _examTerms.GetById(enrollment.TeachingActivityID);
            if (examTerm == null) return false;

            if (DateTime.TryParse(examTerm.Date, out DateTime examDate))
            {
                if ((examDate - DateTime.Now).TotalDays < 2)
                    return false; // previše kasno za otkaz
            }

            _examEnrollments.RemoveExamEnrollment(enrollmentId);
            return true;
        }

        public void DeleteByStudent(string studentId)
            => _examEnrollments.RemoveEnrollmentsByStudent(studentId);

        public ExamEnrollment? GetExamEnrollmentById(int id)
            => _examEnrollments.GetExamEnrollmentById(id);

        public List<int> GetExamTermIds(List<int> examEnrollmentIds)
        {
            var ids = new List<int>();
            foreach (int id in examEnrollmentIds)
            {
                var e = _examEnrollments.GetExamEnrollmentById(id);
                if (e != null) ids.Add(e.TeachingActivityID);
            }
            return ids;
        }

        public void Subscribe(IObserver observer)
            => _examEnrollments.ExamEnrollmentSubject.Subscribe(observer);
    }
}
