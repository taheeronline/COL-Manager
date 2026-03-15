using COLManager.Web.Common;
using COLManager.Web.Data;
using COLManager.Web.DTOs;
using COLManager.Web.Entities;
using Microsoft.EntityFrameworkCore;

namespace COLManager.Web.Services
{
    public class UsageService : IUsageService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<UsageService> _log;

        public UsageService(ApplicationDbContext db, ILogger<UsageService> log)
        {
            _db = db;
            _log = log;
        }

        public async Task<ServiceResult<ColumnUsageReadDto>> CreateAsync(ColumnUsageCreateDto dto)
        {
            try
            {
                if (dto.RuntimeHours <= 0)
                    return ServiceResult<ColumnUsageReadDto>.Fail("Runtime hours must be greater than zero.");
                if (dto.NumberOfInjections < 0)
                    return ServiceResult<ColumnUsageReadDto>.Fail("Number of injections cannot be negative.");

                var column = await _db.Column_Master.FindAsync(dto.ColumnID);
                if (column == null)
                    return ServiceResult<ColumnUsageReadDto>.Fail("Column not found for the provided ColumnID.");

                var entity = new ColumnUsageLog
                {
                    ColumnID = dto.ColumnID,
                    RuntimeHours = dto.RuntimeHours,
                    NumberOfInjections = dto.NumberOfInjections,
                    PreUseBackPressureBar = dto.PreUseBackPressureBar,
                    PostUseBackPressureBar = dto.PostUseBackPressureBar,
                    MaxPressureObservedBar = dto.MaxPressureObservedBar,
                    FlowRateMLPerMin = dto.FlowRateMLPerMin,
                    InjectionVolumeML = dto.InjectionVolumeML,
                    RunDate = dto.RunDate,
                    Remarks = dto.Remarks
                };

                _db.Column_Usage_Log.Add(entity);

                // update totals on column (unit of work within same DbContext)
                column.TotalRuntimeHours += dto.RuntimeHours;
                column.TotalInjections += dto.NumberOfInjections;

                await _db.SaveChangesAsync();

                var read = new ColumnUsageReadDto
                {
                    UsageID = entity.UsageID,
                    ColumnID = entity.ColumnID,
                    RuntimeHours = entity.RuntimeHours,
                    NumberOfInjections = entity.NumberOfInjections,
                    PreUseBackPressureBar = entity.PreUseBackPressureBar,
                    PostUseBackPressureBar = entity.PostUseBackPressureBar,
                    MaxPressureObservedBar = entity.MaxPressureObservedBar,
                    FlowRateMLPerMin = entity.FlowRateMLPerMin,
                    InjectionVolumeML = entity.InjectionVolumeML,
                    RunDate = entity.RunDate,
                    Remarks = entity.Remarks
                };

                return ServiceResult<ColumnUsageReadDto>.Ok(read, "Usage logged successfully.");
            }
            catch (DbUpdateException dbEx)
            {
                _log.LogError(dbEx, "DB error while creating usage log");
                return ServiceResult<ColumnUsageReadDto>.Fail("Database error while creating usage entry.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unexpected error while creating usage log");
                return ServiceResult<ColumnUsageReadDto>.Fail("An unexpected error occurred while creating usage entry.");
            }
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            try
            {
                var entity = await _db.Column_Usage_Log.FindAsync(id);
                if (entity == null)
                    return ServiceResult<bool>.Fail("Usage record not found.");

                // update column totals
                var column = await _db.Column_Master.FindAsync(entity.ColumnID);
                if (column != null)
                {
                    column.TotalRuntimeHours -= entity.RuntimeHours;
                    column.TotalInjections -= entity.NumberOfInjections;
                    if (column.TotalRuntimeHours < 0) column.TotalRuntimeHours = 0;
                    if (column.TotalInjections < 0) column.TotalInjections = 0;
                }

                _db.Column_Usage_Log.Remove(entity);
                await _db.SaveChangesAsync();
                return ServiceResult<bool>.Ok(true, "Usage record deleted.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error deleting usage {Id}", id);
                return ServiceResult<bool>.Fail("An error occurred while deleting the usage record.");
            }
        }

        public async Task<ServiceResult<IEnumerable<ColumnUsageReadDto>>> GetAllAsync(int? columnId = null)
        {
            try
            {
                var query = _db.Column_Usage_Log.AsQueryable();
                if (columnId.HasValue)
                    query = query.Where(x => x.ColumnID == columnId.Value);

                var list = await query.OrderByDescending(x => x.RunDate).ToListAsync();
                var dto = list.Select(e => new ColumnUsageReadDto
                {
                    UsageID = e.UsageID,
                    ColumnID = e.ColumnID,
                    RuntimeHours = e.RuntimeHours,
                    NumberOfInjections = e.NumberOfInjections,
                    PreUseBackPressureBar = e.PreUseBackPressureBar,
                    PostUseBackPressureBar = e.PostUseBackPressureBar,
                    MaxPressureObservedBar = e.MaxPressureObservedBar,
                    FlowRateMLPerMin = e.FlowRateMLPerMin,
                    InjectionVolumeML = e.InjectionVolumeML,
                    RunDate = e.RunDate,
                    Remarks = e.Remarks
                });
                return ServiceResult<IEnumerable<ColumnUsageReadDto>>.Ok(dto);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error fetching usage logs");
                return ServiceResult<IEnumerable<ColumnUsageReadDto>>.Fail("An error occurred while retrieving usage logs.");
            }
        }

        public async Task<ServiceResult<ColumnUsageReadDto>> GetByIdAsync(int id)
        {
            try
            {
                var e = await _db.Column_Usage_Log.FindAsync(id);
                if (e == null)
                    return ServiceResult<ColumnUsageReadDto>.Fail("Usage record not found.");

                var dto = new ColumnUsageReadDto
                {
                    UsageID = e.UsageID,
                    ColumnID = e.ColumnID,
                    RuntimeHours = e.RuntimeHours,
                    NumberOfInjections = e.NumberOfInjections,
                    PreUseBackPressureBar = e.PreUseBackPressureBar,
                    PostUseBackPressureBar = e.PostUseBackPressureBar,
                    MaxPressureObservedBar = e.MaxPressureObservedBar,
                    FlowRateMLPerMin = e.FlowRateMLPerMin,
                    InjectionVolumeML = e.InjectionVolumeML,
                    RunDate = e.RunDate,
                    Remarks = e.Remarks
                };
                return ServiceResult<ColumnUsageReadDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error fetching usage by id {Id}", id);
                return ServiceResult<ColumnUsageReadDto>.Fail("An error occurred while retrieving the usage record.");
            }
        }
    }
}
