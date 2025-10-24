using LangLang.Model.DAO.TeachingActivities;
using LangLang.Model.TeachingActivities;
using System.Collections.Generic;
using LangLang.SQL.Repositories;
using LangLang.Model.TeachingActivities;
using System.Linq;

namespace LangLang.Controller.TeachingActivities
{
    internal class PenaltyPointsController
    {
        private PenaltyPointsDAO _penaltyPointsDAO;
        private readonly PenaltyPointRepository _sqlRepo = new PenaltyPointRepository();

        public void AddPenaltyPointSQL(PenaltyPoint point)
        {
            _sqlRepo.Add(point); 
        }

        public List<PenaltyPoint> GetAllPenaltyPointsSQL()
        {
            return _sqlRepo.GetAll();
        }
        public PenaltyPointsController()
        {
            _penaltyPointsDAO = new PenaltyPointsDAO();
        }
        public List<PenaltyPoint> GetByCourseIdSQL(int courseId)
        {
            return _sqlRepo.GetAll().Where(p => p.CourseId == courseId).ToList();
        }
        public void AddPenaltyPoint(PenaltyPoint penaltyPoint)
        {
            _penaltyPointsDAO.AddPenaltyPoint(penaltyPoint);
        }
        public List<PenaltyPoint> GetByCourseId(int courseId)
        {
            return _penaltyPointsDAO.GetByCourseId(courseId);
        }
    }
}
