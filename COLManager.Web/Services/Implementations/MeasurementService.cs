using COLManager.Web.Common;
using COLManager.Web.Data;
using COLManager.Web.Entities;
using Microsoft.EntityFrameworkCore;

namespace COLManager.Web.Services
{
    public class MeasurementService : IMeasurementService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<MeasurementService> _log;

        public MeasurementService(ApplicationDbContext db, ILogger<MeasurementService> log)
        {
            _db = db;
            _log = log;
        }

        public async Task<ServiceResult<MeasurementType>> CreateAsync(MeasurementType m)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(m.MeasurementName))
                    return ServiceResult<MeasurementType>.Fail("MeasurementName is required.");

                var exists = await _db.Measurement_Type.AnyAsync(x => x.MeasurementName == m.MeasurementName);
                if (exists) return ServiceResult<MeasurementType>.Fail("Measurement name already exists.");

                _db.Measurement_Type.Add(m);
                await _db.SaveChangesAsync();
                return ServiceResult<MeasurementType>.Ok(m, "Measurement created.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error creating measurement");
                return ServiceResult<MeasurementType>.Fail("An error occurred while creating measurement type.");
            }
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            try
            {
                var e = await _db.Measurement_Type.FindAsync(id);
                if (e == null) return ServiceResult<bool>.Fail("Measurement not found.");
                var used = await _db.Unit_Master.AnyAsync(u => u.MeasurementTypeID == id);
                if (used) return ServiceResult<bool>.Fail("Cannot delete measurement type used by units.");
                _db.Measurement_Type.Remove(e);
                await _db.SaveChangesAsync();
                return ServiceResult<bool>.Ok(true, "Deleted.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error deleting measurement");
                return ServiceResult<bool>.Fail("An error occurred while deleting measurement type.");
            }
        }

        public async Task<ServiceResult<IEnumerable<MeasurementType>>> GetAllAsync()
        {
            try
            {
                var list = await _db.Measurement_Type.OrderBy(m => m.MeasurementName).ToListAsync();
                return ServiceResult<IEnumerable<MeasurementType>>.Ok(list);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error fetching measurements");
                return ServiceResult<IEnumerable<MeasurementType>>.Fail("An error occurred while retrieving measurement types.");
            }
        }

        public async Task<ServiceResult<MeasurementType>> GetByIdAsync(int id)
        {
            try
            {
                var e = await _db.Measurement_Type.FindAsync(id);
                if (e == null) return ServiceResult<MeasurementType>.Fail("Measurement not found.");
                return ServiceResult<MeasurementType>.Ok(e);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error fetching measurement {Id}", id);
                return ServiceResult<MeasurementType>.Fail("An error occurred while retrieving measurement type.");
            }
        }

        public async Task<ServiceResult<MeasurementType>> UpdateAsync(MeasurementType m)
        {
            try
            {
                var e = await _db.Measurement_Type.FindAsync(m.MeasurementTypeID);
                if (e == null) return ServiceResult<MeasurementType>.Fail("Measurement not found.");
                e.MeasurementName = m.MeasurementName;
                await _db.SaveChangesAsync();
                return ServiceResult<MeasurementType>.Ok(e, "Updated.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error updating measurement");
                return ServiceResult<MeasurementType>.Fail("An error occurred while updating measurement type.");
            }
        }
    }
}
