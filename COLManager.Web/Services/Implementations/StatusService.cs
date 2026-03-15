using COLManager.Web.Common;
using COLManager.Web.Data;
using COLManager.Web.Entities;
using Microsoft.EntityFrameworkCore;

namespace COLManager.Web.Services
{
    public class StatusService : IStatusService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<StatusService> _log;

        public StatusService(ApplicationDbContext db, ILogger<StatusService> log)
        {
            _db = db;
            _log = log;
        }

        public async Task<ServiceResult<StatusMaster>> CreateAsync(StatusMaster s)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(s.StatusName))
                    return ServiceResult<StatusMaster>.Fail("StatusName is required.");

                var exists = await _db.Status_Master.AnyAsync(x => x.StatusName == s.StatusName);
                if (exists) return ServiceResult<StatusMaster>.Fail("Status name already exists.");

                _db.Status_Master.Add(s);
                await _db.SaveChangesAsync();
                return ServiceResult<StatusMaster>.Ok(s, "Status created.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error creating status");
                return ServiceResult<StatusMaster>.Fail("An error occurred while creating status.");
            }
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            try
            {
                var e = await _db.Status_Master.FindAsync(id);
                if (e == null) return ServiceResult<bool>.Fail("Status not found.");
                var used = await _db.Column_Master.AnyAsync(c => c.StatusID == id);
                if (used) return ServiceResult<bool>.Fail("Cannot delete status used by columns.");
                _db.Status_Master.Remove(e);
                await _db.SaveChangesAsync();
                return ServiceResult<bool>.Ok(true, "Deleted.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error deleting status");
                return ServiceResult<bool>.Fail("An error occurred while deleting status.");
            }
        }

        public async Task<ServiceResult<IEnumerable<StatusMaster>>> GetAllAsync()
        {
            try
            {
                var list = await _db.Status_Master.OrderBy(s => s.StatusName).ToListAsync();
                return ServiceResult<IEnumerable<StatusMaster>>.Ok(list);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error fetching status list");
                return ServiceResult<IEnumerable<StatusMaster>>.Fail("An error occurred while retrieving statuses.");
            }
        }

        public async Task<ServiceResult<StatusMaster>> GetByIdAsync(int id)
        {
            try
            {
                var e = await _db.Status_Master.FindAsync(id);
                if (e == null) return ServiceResult<StatusMaster>.Fail("Status not found.");
                return ServiceResult<StatusMaster>.Ok(e);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error fetching status {Id}", id);
                return ServiceResult<StatusMaster>.Fail("An error occurred while retrieving status.");
            }
        }

        public async Task<ServiceResult<StatusMaster>> UpdateAsync(StatusMaster s)
        {
            try
            {
                var e = await _db.Status_Master.FindAsync(s.StatusID);
                if (e == null) return ServiceResult<StatusMaster>.Fail("Status not found.");
                e.StatusName = s.StatusName;
                await _db.SaveChangesAsync();
                return ServiceResult<StatusMaster>.Ok(e, "Updated.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error updating status");
                return ServiceResult<StatusMaster>.Fail("An error occurred while updating status.");
            }
        }
    }
}
