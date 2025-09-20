using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using LangLang.Controller;
using LangLang.Controller.TeachingActivities;
using LangLang.Controller.Users;
using LangLang.DTO;
using LangLang.Model.Enums;
using LangLang.Model.TeachingActivities;
using LangLang.Model.Users;

namespace LangLang.RoleWindows
{
    public class PrincipalProfessorManager
    {
        private readonly ProfessorsController _professorsController;

        public class DuplicateEmailException : Exception
        {
            public DuplicateEmailException(string message) : base(message) { }
        }

        public class ValidationException : Exception
        {
            public ValidationException(string message) : base(message) { }
        }

        public PrincipalProfessorManager(ProfessorsController professorsController)
        {
            _professorsController = professorsController;
        }

        public void SaveProfessor(ProfessorDTO professorDto, Dictionary<Language, LanguageLevel> professorLanguages, bool isEditMode, ProfessorDTO? originalProfessor = null)
        {
            // Validation
            ValidateProfessorData(professorDto, professorLanguages, isEditMode);

            if (isEditMode && originalProfessor != null)
            {
                UpdateExistingProfessor(professorDto, originalProfessor);
            }
            else
            {
                CreateNewProfessor(professorDto, professorLanguages);
            }
        }

        private void ValidateProfessorData(ProfessorDTO professor, Dictionary<Language, LanguageLevel> languages, bool isEditMode)
        {
            if (!professor.IsValid)
            {
                throw new ValidationException("Please correct validation errors before saving.");
            }

            if (!isEditMode && languages.Count == 0)
            {
                throw new ValidationException("Please add at least one language for the professor.");
            }
        }

        private void CreateNewProfessor(ProfessorDTO professorDto, Dictionary<Language, LanguageLevel> professorLanguages)
        {
            var professor = professorDto.ToProfessor();

            if (EmailExists(professor.Email))
            {
                throw new DuplicateEmailException("An account with this email already exists. Please use a different email.");
            }

            professor.DateOfRegistration = DateTime.Now.ToString("yyyy-MM-dd");
            professor.Languages = new Dictionary<Language, LanguageLevel>(professorLanguages);
            _professorsController.Add(professor);
        }

        private void UpdateExistingProfessor(ProfessorDTO professorDto, ProfessorDTO originalProfessor)
        {
            var updatedProfessor = professorDto.ToProfessor();
            updatedProfessor.DateOfRegistration = originalProfessor.DateOfRegistration;
            updatedProfessor.Languages = originalProfessor.Languages;
            _professorsController.Update(updatedProfessor);
        }

        private bool EmailExists(string email)
        {
            return _professorsController.GetProfessorById(email) != null;
        }

        public bool DeleteProfessor(Professor professor, ProfessorsController professorsController)
        {
            try
            {
                if (!HandleProfessorCourses(professor))
                {
                    return false; // Deletion was cancelled due to course reassignment issues
                }

                HandleProfessorExams(professor);

                professorsController.Delete(professor.Email);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during professor deletion: {ex.Message}",
                                "Deletion Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private bool HandleProfessorCourses(Professor professor)
        {
            var courseController = new CourseController();
            var professorCourses = courseController.GetMyCourses(professor.Email);

            var ongoingCourses = new List<Course>();

            foreach (var course in professorCourses)
            {
                try
                {
                    DateTime courseDate = ParseDate(course.Date);

                    if (courseDate > DateTime.Now)
                    {
                        // Future course - can be deleted safely
                        courseController.ForceDelete(course.Id);
                    }
                    else
                    {
                        // Ongoing course - needs reassignment
                        ongoingCourses.Add(course);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error while processing course {course.Id}: {ex.Message}",
                                    "Course Processing Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            // Handle ongoing courses that need reassignment
            if (ongoingCourses.Any())
            {
                return HandleCourseReassignment(ongoingCourses, professor, courseController);
            }

            return true;
        }

        private bool HandleCourseReassignment(List<Course> ongoingCourses, Professor professor, CourseController courseController)
        {
            try
            {
                var reassignWindow = new ReassignCoursesWindow(
                    ongoingCourses,
                    professor,
                    courseController,
                    _professorsController);

                var result = reassignWindow.ShowDialog();

                if (result != true || !reassignWindow.WasReassigned)
                {
                    MessageBox.Show("Course reassignment was cancelled. The professor cannot be deleted while having ongoing courses.",
                                    "Deletion Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during course reassignment: {ex.Message}",
                                "Reassignment Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void HandleProfessorExams(Professor professor)
        {
            var examsController = new ExamTermsController();
            var professorExams = examsController.GetMyExamTerms(professor.Email);

            foreach (var exam in professorExams)
            {
                try
                {
                    DateTime examDate = ParseDate(exam.Date);

                    // Only delete future exams
                    if (examDate > DateTime.Now)
                    {
                        examsController.ForceDelete(exam.Id);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error while processing exam {exam.Id}: {ex.Message}",
                                    "Exam Processing Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private DateTime ParseDate(string? dateString)
        {
            if (string.IsNullOrEmpty(dateString))
            {
                throw new ArgumentException("Date string cannot be null or empty");
            }

            return DateTime.TryParse(dateString, out var parsed) ? parsed : DateTime.MinValue;
        }
    }
}