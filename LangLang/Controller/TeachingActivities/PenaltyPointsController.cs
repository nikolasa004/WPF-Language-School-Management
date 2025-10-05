using LangLang.Model.DAO.TeachingActivities;
using LangLang.Model.TeachingActivities;
using System.Collections.Generic;

namespace LangLang.Controller.TeachingActivities
{
    internal class PenaltyPointsController
    {
        private PenaltyPointsDAO _penaltyPointsDAO;
        public PenaltyPointsController()
        {
            _penaltyPointsDAO = new PenaltyPointsDAO();
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
