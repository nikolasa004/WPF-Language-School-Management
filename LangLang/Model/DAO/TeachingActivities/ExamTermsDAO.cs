using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using LangLang.Observer;
using LangLang.Storage;
using System;
using LangLang.Model.TeachingActivities;
namespace LangLang.Model.DAO.TeachingActivity
{
    class ExamTermsDAO
    {
        private readonly List<ExamTerm> _examTerms;
        private readonly Storage<ExamTerm> _storage;
        public Subject ExamTermSubject;
        private readonly SortedSet<int> _freeIds;
        private int _maxId;
        private static readonly CultureInfo Sr = new("sr-RS");
        private const string SrDateFmt = "dd.MM.yyyy";
        private const string IsoDateFmt = "yyyy-MM-dd";
        public ExamTermsDAO()
        {
            _storage = new Storage<ExamTerm>("exam-terms.json");
            _examTerms = _storage.Load();
            foreach (var t in _examTerms)
            {
                if (!string.IsNullOrWhiteSpace(t.Date))
                    t.Date = ToSrOrKeep(t.Date);
            }
            ExamTermSubject = new Subject();
            _freeIds = RecalculateFreeIds(_examTerms);
            _maxId = _examTerms.Count == 0 ? 0 : _examTerms.Max(t => t.Id);
        }

        public List<ExamTerm> GetAll() => _examTerms;

        private SortedSet<int> RecalculateFreeIds(List<ExamTerm> terms)
        {
            var free = new SortedSet<int>();
            if (terms.Count == 0) return free;
            int maxId = terms.Max(t => t.Id);
            var used = new HashSet<int>(terms.Select(t => t.Id));
            for (int i = 1; i <= maxId; i++)
                if (!used.Contains(i)) free.Add(i);
            return free;
        }
        private int GenerateId()
        {
            if (_freeIds.Count > 0)
            {
                int id = _freeIds.Min;
                _freeIds.Remove(id);
                return id;
            }
            return ++_maxId;
        }
        public ExamTerm AddExamTerm(ExamTerm term)
        {
            if (term.Id == 0)
                term.Id = GenerateId();
            if (!string.IsNullOrWhiteSpace(term.Date))
                term.Date = ToSrOrKeep(term.Date);
            _examTerms.Add(term);
            SaveAllIso(); 
            ExamTermSubject.NotifyObservers();
            return term;
        }
        public ExamTerm? UpdateExamTerm(ExamTerm term)
        {
            var old = GetExamTermById(term.Id);
            if (old is null) return null;
            old.ProfessorId = term.ProfessorId;
            old.Language = term.Language;
            old.Level = term.Level;
            old.IsLive = term.IsLive;
            old.MaxAttendees = term.MaxAttendees;
            old.Date = ToSrOrKeep(term.Date); 
            old.StartAt = term.StartAt;
            old.Duration = term.Duration;
            old.RoomId = term.RoomId;
            old.IsLocked = term.IsLocked;
            old.IsArchived = term.IsArchived;
            SaveAllIso(); 
            ExamTermSubject.NotifyObservers();
            return old;
        }
        public ExamTerm? RemoveExamTerm(int id)
        {
            var term = GetExamTermById(id);
            if (term is null) return null;
            _examTerms.Remove(term);
            _freeIds.Add(id);
            SaveAllIso();
            ExamTermSubject.NotifyObservers();
            return term;
        }
        public ExamTerm? GetExamTermById(int id)
            => _examTerms.Find(t => t.Id == id);
        public List<ExamTerm> GetAllExamTerms()
            => _examTerms;
        public List<ExamTerm> GetExamTermsByProfessor(string professorId)
            => _examTerms.FindAll(t => t.ProfessorId == professorId);
        private void SaveAllIso()
        {
            var isoCopy = _examTerms.Select(CloneWithIsoDate).ToList();
            _storage.Save(isoCopy);
        }
        private static ExamTerm CloneWithIsoDate(ExamTerm src)
        {
            return new ExamTerm
            {
                Id = src.Id,
                ProfessorId = src.ProfessorId,
                Language = src.Language,
                Level = src.Level,
                IsLive = src.IsLive,
                MaxAttendees = src.MaxAttendees,
                Date = ToIsoOrKeep(src.Date),
                StartAt = src.StartAt,
                Duration = src.Duration,
                RoomId = src.RoomId,
                IsLocked = src.IsLocked,
                IsArchived = src.IsArchived

            };
        }
        private static string ToIsoOrKeep(string? date)
        {
            if (string.IsNullOrWhiteSpace(date)) return date ?? string.Empty;
            if (DateTime.TryParseExact(date, IsoDateFmt, CultureInfo.InvariantCulture,DateTimeStyles.None, out var isoParsed))
                return isoParsed.ToString(IsoDateFmt, CultureInfo.InvariantCulture);
            if (DateTime.TryParseExact(date, SrDateFmt, Sr,DateTimeStyles.None, out var srParsed))
                return srParsed.ToString(IsoDateFmt, CultureInfo.InvariantCulture);
            if (DateTime.TryParse(date, out var any))
                return any.ToString(IsoDateFmt, CultureInfo.InvariantCulture);
            return date;
        }
        private static string ToSrOrKeep(string? date)
        {
            if (string.IsNullOrWhiteSpace(date)) return date ?? string.Empty;
            if (DateTime.TryParseExact(date, IsoDateFmt, CultureInfo.InvariantCulture,
                                       DateTimeStyles.None, out var isoParsed))
                return isoParsed.ToString(SrDateFmt, Sr);
            if (DateTime.TryParseExact(date, SrDateFmt, Sr,DateTimeStyles.None, out var srParsed))
                return srParsed.ToString(SrDateFmt, Sr);
            // fallback: probaj bilo koji format i prikaži kao SR
            if (DateTime.TryParse(date, out var any))
                return any.ToString(SrDateFmt, Sr);
            return date;
        }
    }
}
