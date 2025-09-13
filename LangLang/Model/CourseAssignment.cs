using System.Collections.Generic;

namespace LangLang.Model
{
    public class CourseAssignment
    {
        public CourseAssignment(
            Course course,
            string daysDisplay,
            List<ProfessorOption> availableProfessors,
            ProfessorOption? selectedProfessor = null)
        {
            Course = course;
            DaysDisplay = daysDisplay;
            AvailableProfessorsForCourse = availableProfessors;
            _selectedProfessor = selectedProfessor;
        }

        public Course Course { get; set; }
        public string DaysDisplay { get; set; }
        public List<ProfessorOption> AvailableProfessorsForCourse { get; set; }

        private ProfessorOption? _selectedProfessor;
        public ProfessorOption? SelectedProfessor
        {
            get => _selectedProfessor;
            set => _selectedProfessor = value;
        }
    }

    public class ProfessorOption
    {
        public ProfessorOption(string email, string displayName)
        {
            Email = email;
            DisplayName = displayName;
        }

        public string Email { get; set; }
        public string DisplayName { get; set; }
    }
}
