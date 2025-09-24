using LangLang.RoleWindows.StudentViewModules;
using LangLang.Utils;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace LangLang.View.StudentViewModules
{
    class StudentViewTeaching
    {
        private string currentCoursePanel = string.Empty;
        private string currentExamPanel = string.Empty;

        private readonly Dictionary<string, Action> coursePanelActions;
        private readonly Dictionary<string, Action> examPanelActions;
        private readonly List<Grid> panels;

        private StudentViewCourses studentViewCourses;
        private StudentViewExams studentViewExams;

        public StudentViewTeaching(List<Grid> panels, Dictionary<string, Action> coursePanelActions, Dictionary<string, Action> examPanelActions, StudentViewCourses studentViewCourses, StudentViewExams studentViewExams)
        {
            this.coursePanelActions = coursePanelActions;
            this.examPanelActions = examPanelActions;
            this.panels= panels;
            this.studentViewCourses = studentViewCourses;
            this.studentViewExams = studentViewExams;
        }

        // Course Terms Panel
        public void ShowCourses(string mode)
        {
            currentCoursePanel = mode;
            coursePanelActions[mode].Invoke();
            GUIHelper.ChangePanel(panels, 3);
        }
        public void ShowAllCourses_Click() => ShowCourses("show all");
        public void ShowRequestedCourses_Click() => ShowCourses("show requested");
        public void ShowActiveCourse_Click() => ShowCourses("show active");
        public void CoursePanelButton_Click()
        {
            if (currentCoursePanel == "show all")
                studentViewCourses.RequestForCourse();
            else if (currentCoursePanel == "show requested")
                studentViewCourses.CancelRequestForCourse();
            else if (currentCoursePanel == "show active")
                studentViewCourses.UnenrollRequestCourse();
        }

        // Exam Terms Panel
        public void ShowExams(string mode)
        {
            currentExamPanel = mode;
            examPanelActions[mode].Invoke();
            GUIHelper.ChangePanel(panels, 2);
        }
        public void ShowAllExams_Click() => ShowExams("show all");
        public void ShowRequestedExams_Click() => ShowExams("show requested");
        public void ShowActiveExam_Click() => ShowExams("show active");
        public void ExamPanelButton_Click()
        {
            if (currentExamPanel == "show all")
                studentViewExams.RequestForExam();
            else if (currentExamPanel == "show requested" || currentExamPanel == "show active")
                studentViewExams.CancelRequestForExam();
        }
    }
}