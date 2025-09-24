using LangLang.Controller.Users;
using LangLang.DTO;
using LangLang.Model.DAO.TeachingActivity;

namespace LangLang.Controller.TeachingActivities
{
    public class ExamResultsController
    {
        private readonly ExamResultsDAO _examResultsDAO;
        private readonly ExamTermsController _examTermsController;
        private readonly StudentsController _studentsController;

        public ExamResultsController()
        {
            _examResultsDAO = new ExamResultsDAO();
            _examTermsController = new ExamTermsController();
            _studentsController = new StudentsController();
        }

        public void Save(ExamResultDTO dto)
        {
            var result = dto.ToExamResult();
            _examResultsDAO.AddExamResult(result);
        }
        public ExamResultDTO? GetResultForStudent(int examId, string studentId)
        {
            var result = _examResultsDAO.GetByExamAndStudent(examId, studentId);
            if (result == null)
                return null;

            var exam = _examTermsController.GetById(result.ExamId);
            var student = _studentsController.GetStudentById(result.StudentId);

            if (exam == null || student == null)
                return null;

            var examDto = new ExamTermDTO(exam);
            var studentDto = new StudentDTO(student);

            return new ExamResultDTO(result, examDto, studentDto);
        }

    }
}
