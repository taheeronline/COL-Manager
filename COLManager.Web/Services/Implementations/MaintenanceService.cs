using COLManager.Web.Common;
using COLManager.Web.Data;
using COLManager.Web.Entities;
using COLManager.Web.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace COLManager.Web.Services
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<MaintenanceService> _log;
        private readonly IAuditService _auditService;

        public MaintenanceService(ApplicationDbContext db, ILogger<MaintenanceService> log, IAuditService auditService)
        {
            _db = db;
            _log = log;
            _auditService = auditService;
        }

        public async Task<ServiceResult<ColumnMaintenanceLog>> CreateAsync(ColumnMaintenanceLog m)
        {
            try
            {
                var column = await _db.Column_Master.FindAsync(m.ColumnID);
                if (column == null) 
                    return ServiceResult<ColumnMaintenanceLog>.Fail("Column not found.");

                // Check if column is in UnderMaintenance status
                if (column.StatusID != (int)ColumnStatus.UnderMaintenance && column.StatusID != (int)ColumnStatus.Available)
                    return ServiceResult<ColumnMaintenanceLog>.Fail("Only columns with 'Under Maintenance' status can have maintenance entries.");

                if (string.IsNullOrWhiteSpace(m.MaintenanceType))
                    return ServiceResult<ColumnMaintenanceLog>.Fail("MaintenanceType is required.");

                if (string.IsNullOrWhiteSpace(m.PerformedBy))
                    return ServiceResult<ColumnMaintenanceLog>.Fail("PerformedBy is required.");

                m.PerformedOn = m.PerformedOn == default ? DateTime.UtcNow : m.PerformedOn;

                _db.Column_Maintenance_Log.Add(m);
                await _db.SaveChangesAsync();

                // Change column status to Available after maintenance is logged
                var oldColumn = new ColumnMaster
                {
                    ColumnID = column.ColumnID,
                    ColumnName = column.ColumnName,
                    Manufacturer = column.Manufacturer,
                    SerialNumber = column.SerialNumber,
                    LengthMM = column.LengthMM,
                    InternalDiameterMM = column.InternalDiameterMM,
                    ParticleSizeMicron = column.ParticleSizeMicron,
                    MaxPressureBar = column.MaxPressureBar,
                    ProtocolID = column.ProtocolID,
                    StatusID = column.StatusID,
                    TotalRuntimeHours = column.TotalRuntimeHours,
                    TotalInjections = column.TotalInjections,
                    InstalledOn = column.InstalledOn,
                    RetiredOn = column.RetiredOn,
                    CreatedOn = column.CreatedOn
                };

                column.StatusID = (int)ColumnStatus.Available;
                await _db.SaveChangesAsync();

                // Log audit trail for status change
                await LogAuditMaintenanceAsync(m, oldColumn, column);

                _log.LogInformation("Maintenance logged and column status changed to Available: Column {ColumnName} (ID: {ColumnID})", 
                    column.ColumnName, column.ColumnID);

                return ServiceResult<ColumnMaintenanceLog>.Ok(m, "Maintenance logged and column status changed to Available.");
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
                var list = await _db.Column_Maintenance_Log
                    .Where(m => m.ColumnID == columnId)
                    .OrderByDescending(m => m.PerformedOn)
                    .ToListAsync();
                return ServiceResult<IEnumerable<ColumnMaintenanceLog>>.Ok(list);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error fetching maintenance for column {ColumnId}", columnId);
                return ServiceResult<IEnumerable<ColumnMaintenanceLog>>.Fail("An error occurred while retrieving maintenance records.");
            }
        }

        private async Task LogAuditMaintenanceAsync(ColumnMaintenanceLog maintenance, ColumnMaster oldColumn, ColumnMaster newColumn)
        {
            try
            {
                var oldStatusName = await GetStatusNameAsync(oldColumn.StatusID);
                var newStatusName = await GetStatusNameAsync(newColumn.StatusID);

                var maintenanceData = new
                {
                    MaintenanceType = maintenance.MaintenanceType,
                    Description = maintenance.Description,
                    PerformedBy = maintenance.PerformedBy,
                    PerformedOn = maintenance.PerformedOn
                };

                var changeData = new
                {
                    Maintenance = maintenanceData,
                    StatusChange = new
                    {
                        Old = oldStatusName,
                        New = newStatusName
                    }
                };

                var audit = new AuditTrail
                {
                    TableName = "Column_Master",
                    RecordID = newColumn.ColumnID,
                    OperationType = "MAINTENANCE",
                    OldData = null,
                    NewData = JsonSerializer.Serialize(changeData),
                    ChangedBy = "System",
                    ChangedOn = DateTime.UtcNow
                };

                await _auditService.CreateAsync(audit);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Failed to log audit trail for maintenance on column {Id}", oldColumn.ColumnID);
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
