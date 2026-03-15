using COLManager.Web.Common;
using COLManager.Web.Data;
using COLManager.Web.Entities;
using Microsoft.EntityFrameworkCore;

namespace COLManager.Web.Services
{
    public class UnitService : IUnitService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<UnitService> _log;

        public UnitService(ApplicationDbContext db, ILogger<UnitService> log)
        {
            _db = db;
            _log = log;
        }

        public async Task<ServiceResult<UnitMaster>> CreateAsync(UnitMaster u)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(u.UnitName) || string.IsNullOrWhiteSpace(u.UnitSymbol))
                    return ServiceResult<UnitMaster>.Fail("UnitName and UnitSymbol are required.");

                // ensure measurement exists
                var m = await _db.Measurement_Type.FindAsync(u.MeasurementTypeID);
                if (m == null) return ServiceResult<UnitMaster>.Fail("Measurement type not found.");

                _db.Unit_Master.Add(u);
                await _db.SaveChangesAsync();
                return ServiceResult<UnitMaster>.Ok(u, "Unit created.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error creating unit");
                return ServiceResult<UnitMaster>.Fail("An error occurred while creating unit.");
            }
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            try
            {
                var e = await _db.Unit_Master.FindAsync(id);
                if (e == null) return ServiceResult<bool>.Fail("Unit not found.");
                _db.Unit_Master.Remove(e);
                await _db.SaveChangesAsync();
                return ServiceResult<bool>.Ok(true, "Deleted.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error deleting unit");
                return ServiceResult<bool>.Fail("An error occurred while deleting unit.");
            }
        }

        public async Task<ServiceResult<IEnumerable<UnitMaster>>> GetAllAsync(int? measurementTypeId = null)
        {
            try
            {
                var q = _db.Unit_Master.AsQueryable();
                if (measurementTypeId.HasValue) q = q.Where(u => u.MeasurementTypeID == measurementTypeId.Value);
                var list = await q.OrderBy(u => u.UnitName).ToListAsync();
                return ServiceResult<IEnumerable<UnitMaster>>.Ok(list);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error fetching units");
                return ServiceResult<IEnumerable<UnitMaster>>.Fail("An error occurred while retrieving units.");
            }
        }

        public async Task<ServiceResult<UnitMaster>> GetByIdAsync(int id)
        {
            try
            {
                var e = await _db.Unit_Master.FindAsync(id);
                if (e == null) return ServiceResult<UnitMaster>.Fail("Unit not found.");
                return ServiceResult<UnitMaster>.Ok(e);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error fetching unit {Id}", id);
                return ServiceResult<UnitMaster>.Fail("An error occurred while retrieving unit.");
            }
        }

        public async Task<ServiceResult<UnitMaster>> UpdateAsync(UnitMaster u)
        {
            try
            {
                var e = await _db.Unit_Master.FindAsync(u.UnitID);
                if (e == null) return ServiceResult<UnitMaster>.Fail("Unit not found.");
                e.UnitName = u.UnitName;
                e.UnitSymbol = u.UnitSymbol;
                e.MeasurementTypeID = u.MeasurementTypeID;
                e.IsBaseUnit = u.IsBaseUnit;
                await _db.SaveChangesAsync();
                return ServiceResult<UnitMaster>.Ok(e, "Updated.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error updating unit");
                return ServiceResult<UnitMaster>.Fail("An error occurred while updating unit.");
            }
        }
    }
}
