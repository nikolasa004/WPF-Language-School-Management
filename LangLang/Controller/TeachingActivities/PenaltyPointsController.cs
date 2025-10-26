using System.Collections.Generic;
using System.Linq;
using System.Windows;
using LangLang.Model.DAO.TeachingActivities;
using LangLang.Model.TeachingActivities;
using LangLang.SQL;
using LangLang.SQL.Repositories;

namespace LangLang.Controller.TeachingActivities
{
    internal class PenaltyPointsController
    {
        private PenaltyPointsDAO _penaltyPointsDAO;
        private readonly PenaltyPointRepository _sqlRepo;

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
            _sqlRepo = new PenaltyPointRepository();

            using var context = new LangLangContext();
            context.Database.EnsureCreated();

            SyncJsonToSqlIfEmpty();
        }

        private void SyncJsonToSqlIfEmpty()
        {
            try
            {
                var sqlPenaltyPoints = _sqlRepo.GetAll();
               
                if (sqlPenaltyPoints == null || sqlPenaltyPoints.Count == 0)
                {
                    var jsonPenaltyPoints = _penaltyPointsDAO.GetAll();

                    if (jsonPenaltyPoints != null && jsonPenaltyPoints.Count > 0)
                    {
                        foreach (var point in jsonPenaltyPoints)
                        {
                            _sqlRepo.Add(point);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Warning: Could not sync penalty points from JSON to database.\n\nError: {ex.Message}\n\nStack Trace: {ex.StackTrace}",
                    "Sync Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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