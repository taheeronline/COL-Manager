using COLManager.Web.Common;
using COLManager.Web.Data;
using COLManager.Web.DTOs;
using COLManager.Web.Entities;
using COLManager.Web.Enums;
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
                    .AnyAsync(x => x.ColumnID == dto.ColumnID && x.Status == ColumnStatus.CheckedOut);

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
                    Status = ColumnStatus.CheckedOut,
                    Remarks = dto.Remarks,
                };

                _db.Column_Usage_Log.Add(entity);
                await _db.SaveChangesAsync();

                _db.Column_Master
                    .Where(c => c.ColumnID == dto.ColumnID)
                    .ExecuteUpdate(s => s
                        .SetProperty(
                            c => c.InstalledOn,
                            c => c.InstalledOn == null ? DateTime.UtcNow : c.InstalledOn
                        )
                        .SetProperty(
                            c => c.StatusID,
                            (int)ColumnStatus.CheckedOut // or your desired enum value
                        )
                    );

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
        var usage = await _db.Column_Usage_Log
            .FirstOrDefaultAsync(x => x.UsageID == dto.CheckoutID);

        if (usage == null)
            return ServiceResult<bool>.Fail("Checkout not found.");

        // Update usage log
        usage.CheckinDate = dto.CheckinDate;
        usage.Status = (ColumnStatus)dto.Status;
        usage.RuntimeHours = dto.RuntimeHours;
        usage.NumberOfInjections = dto.NumberOfInjections;
        usage.PreUseBackPressureBar = dto.PreUseBackPressureBar;
        usage.PostUseBackPressureBar = dto.PostUseBackPressureBar;
        usage.MaxPressureObservedBar = dto.MaxPressureObservedBar;
        usage.FlowRateMLPerMin = dto.FlowRateMLPerMin;
        usage.InjectionVolumeML = dto.InjectionVolumeML;
        usage.Remarks = dto.Remarks;

        // 🔥 Update Column status
        var column = await _db.Column_Master
            .FirstOrDefaultAsync(c => c.ColumnID == usage.ColumnID);

        if (column != null && dto.Status.HasValue)
        {
            column.StatusID = (int)dto.Status.Value;

            if (dto.Status == ColumnStatus.Retired)
                column.RetiredOn = DateTime.UtcNow;

            // Update totals
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
                    Status = (ColumnStatus)u.Status,
                    ColumnName = c.ColumnName,
                    SerialNumber = c.SerialNumber,
                    ProtocolName = c.ProtocolID != null ? _db.Protocol.Where(p => p.ProtocolID == c.ProtocolID).Select(p => p.ProtocolName).FirstOrDefault() : "N/A",
                    ColumnMaxPressureBar = c.MaxPressureBar
                }
            ).ToListAsync();
        }

        public async Task<List<ColumnCheckoutReadDto>> GetCheckedOutOnlyAsync()
        {
            return await (
                from u in _db.Column_Usage_Log
                join c in _db.Column_Master on u.ColumnID equals c.ColumnID
                join p in _db.Protocol on c.ProtocolID equals p.ProtocolID 
                where u.Status == ColumnStatus.CheckedOut
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
                    MaxAllowedPressureBar = p.MaxAllowedPressureBar,
                    ColumnMaxPressureBar = c.MaxPressureBar
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