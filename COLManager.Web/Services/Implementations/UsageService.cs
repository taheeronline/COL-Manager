using COLManager.Web.Common;
using COLManager.Web.Data;
using COLManager.Web.DTOs;
using COLManager.Web.Entities;
using COLManager.Web.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace COLManager.Web.Services
{
    public class UsageService : IUsageService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<UsageService> _log;
        private readonly IAuditService _auditService;

        public UsageService(ApplicationDbContext db, ILogger<UsageService> log, IAuditService auditService)
        {
            _db = db;
            _log = log;
            _auditService = auditService;
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

                // 4️⃣ Store old status for audit trail
                var oldStatusName = await GetStatusNameAsync(column.StatusID);

                // 5️⃣ Create checkout record
                var entity = new ColumnUsageLog
                {
                    ColumnID = dto.ColumnID,
                    CheckoutDate = dto.CheckoutDate,
                    Status = ColumnStatus.CheckedOut,
                    Remarks = dto.Remarks,
                };

                _db.Column_Usage_Log.Add(entity);
                await _db.SaveChangesAsync();

                // 6️⃣ Update column status
                column.StatusID = (int)ColumnStatus.CheckedOut;
                await _db.SaveChangesAsync();

                // 7️⃣ Log audit trail for checkout
                await LogAuditCheckoutAsync(column, oldStatusName);

                _log.LogInformation("Checkout successful for column {ColumnName} (ID: {ColumnID})", 
                    column.ColumnName, column.ColumnID);

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

                var column = await _db.Column_Master
                    .FirstOrDefaultAsync(c => c.ColumnID == usage.ColumnID);

                if (column == null)
                    return ServiceResult<bool>.Fail("Column not found.");

                // Store old status for audit trail
                var oldStatusName = await GetStatusNameAsync(column.StatusID);

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

                // Update Column status
                if (dto.Status.HasValue)
                {
                    column.StatusID = (int)dto.Status.Value;

                    if (dto.Status == ColumnStatus.Retired)
                        column.RetiredOn = DateTime.UtcNow;

                    // Update totals
                    column.TotalRuntimeHours += dto.RuntimeHours;
                    column.TotalInjections += dto.NumberOfInjections;
                }

                await _db.SaveChangesAsync();

                // Log audit trail for check-in
                var newStatusName = await GetStatusNameAsync(column.StatusID);
                await LogAuditCheckinAsync(column, usage, oldStatusName, newStatusName, dto);

                _log.LogInformation("Check-in completed for column {ColumnName} (ID: {ColumnID})", 
                    column.ColumnName, column.ColumnID);

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
                where c.StatusID == (int)ColumnStatus.CheckedOut && u.CheckinDate == null
                orderby u.CheckoutDate descending
                select new ColumnCheckoutReadDto
                {
                    CheckoutID = u.UsageID,
                    ColumnID = u.ColumnID,
                    CheckoutDate = u.CheckoutDate ?? DateTime.UtcNow,
                    CheckinDate = u.CheckinDate,
                    Status = (ColumnStatus)c.StatusID,
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
                where c.StatusID == (int)ColumnStatus.CheckedOut && u.CheckinDate == null
                orderby u.CheckoutDate descending
                select new ColumnCheckoutReadDto
                {
                    CheckoutID = u.UsageID,
                    ColumnID = u.ColumnID,
                    CheckoutDate = u.CheckoutDate ?? DateTime.UtcNow,
                    Status = (ColumnStatus)c.StatusID,
                    ColumnName = c.ColumnName,
                    SerialNumber = c.SerialNumber,
                    ProtocolName = c.ProtocolID != null ? _db.Protocol.Where(p => p.ProtocolID == c.ProtocolID).Select(p => p.ProtocolName).FirstOrDefault() : "N/A",
                    MaxAllowedUsageHours = p.MaxAllowedPressureBar,
                    MaxAllowedInjections = p.MaxAllowedInjections,
                    MaxAllowedPressureBar = p.MaxAllowedPressureBar,
                    ColumnMaxPressureBar = c.MaxPressureBar,
                    TotalRuntimeHours = c.TotalRuntimeHours,
                    TotalInjections = c.TotalInjections
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
                    Status = (ColumnStatus)c.StatusID,
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

        // =========================
        // AUDIT TRAIL HELPERS
        // =========================

        private async Task LogAuditCheckoutAsync(ColumnMaster column, string oldStatusName)
        {
            try
            {
                var newStatusName = await GetStatusNameAsync((int)ColumnStatus.CheckedOut);
                var checkoutData = new
                {
                    ColumnName = column.ColumnName,
                    SerialNumber = column.SerialNumber,
                    CheckoutDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    StatusChange = new
                    {
                        From = oldStatusName,
                        To = newStatusName
                    }
                };

                var audit = new AuditTrail
                {
                    TableName = "Column_Master",
                    RecordID = column.ColumnID,
                    OperationType = "CHECKOUT",
                    OldData = null,
                    NewData = JsonSerializer.Serialize(checkoutData),
                    ChangedBy = "System",
                    ChangedOn = DateTime.UtcNow
                };

                await _auditService.CreateAsync(audit);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Failed to log audit trail for checkout on column {Id}", column.ColumnID);
            }
        }

        private async Task LogAuditCheckinAsync(ColumnMaster column, ColumnUsageLog usage, string oldStatusName, string newStatusName, ColumnCheckinDto dto)
        {
            try
            {
                var checkinData = new
                {
                    ColumnName = column.ColumnName,
                    SerialNumber = column.SerialNumber,
                    CheckinDate = dto.CheckinDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    StatusChange = new
                    {
                        From = oldStatusName,
                        To = newStatusName
                    },
                    Usage = new
                    {
                        RuntimeHours = dto.RuntimeHours,
                        NumberOfInjections = dto.NumberOfInjections,
                        PreUseBackPressureBar = dto.PreUseBackPressureBar,
                        PostUseBackPressureBar = dto.PostUseBackPressureBar,
                        MaxPressureObservedBar = dto.MaxPressureObservedBar
                    },
                    Remarks = dto.Remarks
                };

                var audit = new AuditTrail
                {
                    TableName = "Column_Master",
                    RecordID = column.ColumnID,
                    OperationType = "CHECKIN",
                    OldData = null,
                    NewData = JsonSerializer.Serialize(checkinData),
                    ChangedBy = "System",
                    ChangedOn = DateTime.UtcNow
                };

                await _auditService.CreateAsync(audit);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Failed to log audit trail for check-in on column {Id}", column.ColumnID);
            }
        }

        private async Task<string> GetStatusNameAsync(int statusId)
        {
            try
            {
                var status = await _db.Status_Master.FirstOrDefaultAsync(s => s.StatusID == statusId);
                return status?.StatusName ?? $"Status ID {statusId}";
            }
            catch
            {
                return $"Status ID {statusId}";
            }
        }
    }
}