using LangLang.Storage;
using System.Collections.Generic;
using LangLang.Observer;
using LangLang.Model.Users;
using System;

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

        public List<Professor> GetAll() => _professors;
        public void Add(Professor p) => AddProfessor(p);
        public void Update(Professor p) => UpdateProfessor(p);
        public void Remove(Professor p) => RemoveProfessor(p.Email);
        public void SaveChanges() => _storage.Save(_professors);
        public void Load()
        {
            var fresh = _storage.Load();
            _professors.Clear();
            _professors.AddRange(fresh);
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
            oldProfessor.AverageRating = professor.AverageRating;

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

        public void AddRating(string professorEmail, int rating)
        {
            Professor? professor = GetProfessorById(professorEmail);
            if (professor != null)
            {
                int totalRating = professor.AverageRating.Item1 + rating;
                int numberOfRatings = professor.AverageRating.Item2 + 1;
                professor.AverageRating = new Tuple<int, int>(totalRating, numberOfRatings);
                _storage.Save(_professors);
                ProfessorSubject.NotifyObservers();
            }
        }

        public List<Professor> GetAllProfessors()
        {
            return _professors;
        }
    }
}
