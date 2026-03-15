using COLManager.Web.Common;
using COLManager.Web.Data;
using COLManager.Web.Entities;
using Microsoft.EntityFrameworkCore;

namespace COLManager.Web.Services
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<MaintenanceService> _log;

        public MaintenanceService(ApplicationDbContext db, ILogger<MaintenanceService> log)
        {
            _db = db;
            _log = log;
        }

        public async Task<ServiceResult<ColumnMaintenanceLog>> CreateAsync(ColumnMaintenanceLog m)
        {
            try
            {
                var column = await _db.Column_Master.FindAsync(m.ColumnID);
                if (column == null) return ServiceResult<ColumnMaintenanceLog>.Fail("Column not found.");

                if (string.IsNullOrWhiteSpace(m.MaintenanceType))
                    return ServiceResult<ColumnMaintenanceLog>.Fail("MaintenanceType is required.");

                m.PerformedOn = m.PerformedOn == default ? DateTime.UtcNow : m.PerformedOn;

                _db.Column_Maintenance_Log.Add(m);
                await _db.SaveChangesAsync();
                return ServiceResult<ColumnMaintenanceLog>.Ok(m, "Maintenance logged.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error creating maintenance");
                return ServiceResult<ColumnMaintenanceLog>.Fail("An error occurred while logging maintenance.");
            }
        }

        public async Task<ServiceResult<IEnumerable<ColumnMaintenanceLog>>> GetByColumnAsync(int columnId)
        {
            try
            {
                var list = await _db.Column_Maintenance_Log.Where(m => m.ColumnID == columnId).OrderByDescending(m => m.PerformedOn).ToListAsync();
                return ServiceResult<IEnumerable<ColumnMaintenanceLog>>.Ok(list);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error fetching maintenance for column {ColumnId}", columnId);
                return ServiceResult<IEnumerable<ColumnMaintenanceLog>>.Fail("An error occurred while retrieving maintenance records.");
            }
        }
    }
}
