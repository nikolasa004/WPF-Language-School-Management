using System.Collections.Generic;
using LangLang.Model.TeachingActivities;
using LangLang.Storage;

namespace LangLang.Model.DAO.TeachingActivities
{
    internal class PenaltyPointsDAO
    {
        private readonly List<PenaltyPoint> _penaltyPointsList;
        private readonly Storage<PenaltyPoint> _storage;

        public PenaltyPointsDAO()
        {
            _storage = new Storage<PenaltyPoint>("penalty-points.json");
            _penaltyPointsList = _storage.Load();
        }

        public List<PenaltyPoint> GetAll()
        {
            return _penaltyPointsList;
        }
        int GetNextId()
        {
            if (_penaltyPointsList.Count == 0) return 1;
            return _penaltyPointsList[^1].Id + 1;
        }
        public PenaltyPoint AddPenaltyPoint(PenaltyPoint penaltyPoint)
        {
            penaltyPoint.Id = GetNextId();
            _penaltyPointsList.Add(penaltyPoint);
            _storage.Save(_penaltyPointsList);
            return penaltyPoint;
        }
        public bool RemovePenaltyPoint(int id)
        {
            var penaltyPoint = _penaltyPointsList.Find(pp => pp.Id == id);
            if (penaltyPoint == null) return false;
            _penaltyPointsList.Remove(penaltyPoint);
            _storage.Save(_penaltyPointsList);
            return true;
        }
        public PenaltyPoint? GetById(int id)
        {
            return _penaltyPointsList.Find(pp => pp.Id == id);
        }
        public List<PenaltyPoint> GetByCourseId(int courseId)
        {
            return _penaltyPointsList.FindAll(pp => pp.CourseId == courseId);
        }
    }
}
