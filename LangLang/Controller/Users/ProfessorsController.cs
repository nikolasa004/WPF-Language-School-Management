using System;
using System.Collections.Generic;
using LangLang.Model.DAO.Users;
using LangLang.Model.Users;
using LangLang.Observer;

namespace LangLang.Controller.Users
{
    public class ProfessorsController
    {
        private readonly ProfessorsDAO _professors;

        public ProfessorsController()
        {
            _professors = new ProfessorsDAO();
        }

        public List<Professor> GetAllProfessors()
        {
            return _professors.GetAllProfessors();
        }

        public void Add(Professor professor)
        {
            _professors.AddProfessor(professor);
        }
        public Professor? Update(Professor professor)
        {
            return _professors.UpdateProfessor(professor);
        }
        public void Delete(string professorEmail)
        {
            _professors.RemoveProfessor(professorEmail);
        }

        public void AddRating(string professorEmail, int rating)
        {
            _professors.AddRating(professorEmail, rating);
        }

        public Professor? GetProfessorById(string email)
        {
            return _professors.GetProfessorById(email);
        }

        public void Subscribe(IObserver observer)
        {
            _professors.ProfessorSubject.Subscribe(observer);
        }
    }
}