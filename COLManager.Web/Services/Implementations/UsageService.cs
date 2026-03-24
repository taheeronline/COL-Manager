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

        // =========================
        // CHECKOUT
        // =========================

        public async Task<ServiceResult<bool>> CreateCheckoutAsync(ColumnCheckoutCreateDto dto)
        {
            try
            {
                // 1️⃣ Check if column is already checked out
                var exists = await _db.Column_Usage_Log
                    .AnyAsync(x => x.ColumnID == dto.ColumnID && x.Status == "Checked Out");

                if (exists)
                    return ServiceResult<bool>.Fail("Column is already checked out.");

                // 2️⃣ Get column info (including linked protocol)
                var column = await _db.Column_Master
                    .FirstOrDefaultAsync(c => c.ColumnID == dto.ColumnID);

                if (column == null)
                    return ServiceResult<bool>.Fail("Selected column not found.");

                // 3️⃣ Resolve protocol info from column
                var protocol = await _db.Protocol
                    .FirstOrDefaultAsync(p => p.ProtocolID == column.ProtocolID && p.IsActive);

                if (protocol == null)
                    return ServiceResult<bool>.Fail("Protocol for selected column not found or inactive.");

                // 4️⃣ Create checkout record
                var entity = new ColumnUsageLog
                {
                    ColumnID = dto.ColumnID,
                    CheckoutDate = dto.CheckoutDate,
                    Status = "Checked Out",
                    Remarks = dto.Remarks,
                };

                _db.Column_Usage_Log.Add(entity);
                await _db.SaveChangesAsync();

                return ServiceResult<bool>.Ok(true, "Checkout successful.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Checkout failed");
                return ServiceResult<bool>.Fail("Checkout failed.");
            }
        }

        // =========================
        // CHECK-IN
        // =========================

        public async Task<ServiceResult<bool>> CheckinAsync(ColumnCheckinDto dto)
        {
            try
            {
                var entity = await _db.Column_Usage_Log
                    .FirstOrDefaultAsync(x => x.UsageID == dto.CheckoutID);

                if (entity == null)
                    return ServiceResult<bool>.Fail("Record not found.");

                if (entity.Status == "Checked In")
                    return ServiceResult<bool>.Fail("Already checked in.");

                entity.Status = dto.Status;
                entity.CheckinDate = dto.CheckinDate;

                entity.RuntimeHours = dto.RuntimeHours;
                entity.NumberOfInjections = dto.NumberOfInjections;
                entity.PreUseBackPressureBar = dto.PreUseBackPressureBar;
                entity.PostUseBackPressureBar = dto.PostUseBackPressureBar;
                entity.MaxPressureObservedBar = dto.MaxPressureObservedBar;
                entity.FlowRateMLPerMin = dto.FlowRateMLPerMin;
                entity.InjectionVolumeML = dto.InjectionVolumeML;
                entity.RunDate = dto.CheckinDate;
                entity.Remarks = dto.Remarks;

                var column = await _db.Column_Master.FindAsync(entity.ColumnID);
                if (column != null)
                {
                    column.TotalRuntimeHours += dto.RuntimeHours;
                    column.TotalInjections += dto.NumberOfInjections;
                }

                await _db.SaveChangesAsync();

                return ServiceResult<bool>.Ok(true, "Check-in completed.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Check-in failed");
                return ServiceResult<bool>.Fail("Check-in failed.");
            }
        }

        // =========================
        // READ
        // =========================

        public async Task<List<ColumnCheckoutReadDto>> GetCheckoutsAsync()
        {
            return await (
                from u in _db.Column_Usage_Log
                join c in _db.Column_Master on u.ColumnID equals c.ColumnID
                orderby u.CheckoutDate descending
                select new ColumnCheckoutReadDto
                {
                    CheckoutID = u.UsageID,
                    ColumnID = u.ColumnID,
                    CheckoutDate = u.CheckoutDate ?? DateTime.UtcNow,
                    CheckinDate = u.CheckinDate,
                    Status = u.Status,
                    ColumnName = c.ColumnName,
                    SerialNumber = c.SerialNumber
                }
            ).ToListAsync();
        }

        public async Task<List<ColumnCheckoutReadDto>> GetCheckedOutOnlyAsync()
        {
            return await (
                from u in _db.Column_Usage_Log
                join c in _db.Column_Master on u.ColumnID equals c.ColumnID
                join p in _db.Protocol on c.ProtocolID equals p.ProtocolID 
                where u.Status == "Checked Out"
                select new ColumnCheckoutReadDto
                {
                    CheckoutID = u.UsageID,
                    ColumnID = u.ColumnID,
                    CheckoutDate = u.CheckoutDate ?? DateTime.UtcNow,
                    Status = u.Status,
                    ColumnName = c.ColumnName,
                    SerialNumber = c.SerialNumber,
                    ProtocolName=c.ProtocolID != null ? _db.Protocol.Where(p => p.ProtocolID == c.ProtocolID).Select(p => p.ProtocolName).FirstOrDefault() : "N/A",
                    MaxAllowedUsageHours = p.MaxAllowedPressureBar,
                    MaxAllowedInjections = p.MaxAllowedInjections,
                    MaxAllowedPressureBar = p.MaxAllowedPressureBar
                }
            ).ToListAsync();
        }

        public async Task<List<ColumnUsageLifecycleReadDto>> GetLifecycleAsync()
        {
            return await (
                from u in _db.Column_Usage_Log
                join c in _db.Column_Master on u.ColumnID equals c.ColumnID
                orderby u.CheckoutDate descending
                select new ColumnUsageLifecycleReadDto
                {
                    CheckoutID = u.UsageID,
                    ColumnID = u.ColumnID,
                    ColumnName = c.ColumnName,
                    SerialNumber = c.SerialNumber,
                    Status = u.Status,
                    CheckoutDate = u.CheckoutDate ?? DateTime.UtcNow,
                    CheckinDate = u.CheckinDate,

                    RuntimeHours = u.RuntimeHours,
                    NumberOfInjections = u.NumberOfInjections,
                    PreUseBackPressureBar = u.PreUseBackPressureBar,
                    PostUseBackPressureBar = u.PostUseBackPressureBar,
                    MaxPressureObservedBar = u.MaxPressureObservedBar,
                    FlowRateMLPerMin = u.FlowRateMLPerMin,
                    InjectionVolumeML = u.InjectionVolumeML,
                    Remarks = u.Remarks
                }
            ).ToListAsync();
        }
    }
}