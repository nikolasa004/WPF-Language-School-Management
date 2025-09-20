using LangLang.Storage;
using System.Collections.Generic;
using LangLang.Observer;
using LangLang.Model.Users;

namespace LangLang.Model.DAO.Users
{
    class ProfessorsDAO
    {
        private readonly List<Professor> _professors;
        private readonly Storage<Professor> _storage;

        public Subject ProfessorSubject;

        public ProfessorsDAO()
        {
            _storage = new Storage<Professor>("professors.json");
            _professors = _storage.Load();
            ProfessorSubject = new Subject();
        }

        public Professor AddProfessor(Professor professor)
        {
            _professors.Add(professor);
            _storage.Save(_professors);
            ProfessorSubject.NotifyObservers();
            return professor;
        }

        public Professor? UpdateProfessor(Professor professor)
        {
            Professor? oldProfessor = GetProfessorById(professor.Email);
            if (oldProfessor is null) return null;

            oldProfessor.FirstName = professor.FirstName;
            oldProfessor.LastName = professor.LastName;
            oldProfessor.Gender = professor.Gender;
            oldProfessor.DateOfBirth = professor.DateOfBirth;
            oldProfessor.PhoneNumber = professor.PhoneNumber;
            oldProfessor.Password = professor.Password;
            oldProfessor.EducationLevel = professor.EducationLevel;
            oldProfessor.DateOfRegistration = professor.DateOfRegistration;
            oldProfessor.Languages = professor.Languages;

            _storage.Save(_professors);
            ProfessorSubject.NotifyObservers();
            return oldProfessor;
        }

        public Professor? RemoveProfessor(string email)
        {
            Professor? professor = GetProfessorById(email);
            if (professor == null) return null;

            _professors.Remove(professor);
            _storage.Save(_professors);
            ProfessorSubject.NotifyObservers();
            return professor;
        }

        public Professor? GetProfessorById(string email)
        {
            return _professors.Find(p => p.Email == email);
        }

        public List<Professor> GetAllProfessors()
        {
            return _professors;
        }
    }
}
