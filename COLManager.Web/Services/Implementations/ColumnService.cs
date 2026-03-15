using COLManager.Web.Common;
using COLManager.Web.Data;
using COLManager.Web.DTOs;
using COLManager.Web.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace COLManager.Web.Services
{
    public class ColumnService : IColumnService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ColumnService> _log;
        private readonly IAuditService _auditService;

        public ColumnService(ApplicationDbContext db, ILogger<ColumnService> log, IAuditService auditService)
        {
            _db = db;
            _log = log;
            _auditService = auditService;
        }

        public async Task<ServiceResult<IEnumerable<ColumnMasterReadDto>>> GetAllAsync()
        {
            try
            {
                _log.LogInformation("Retrieving all columns from database");
                var items = await _db.Column_Master.OrderBy(c => c.ColumnID).ToListAsync();
                var dto = items.Select(c => new ColumnMasterReadDto
                {
                    ColumnID = c.ColumnID,
                    ColumnName = c.ColumnName,
                    Manufacturer = c.Manufacturer,
                    SerialNumber = c.SerialNumber,
                    LengthMM = c.LengthMM,
                    InternalDiameterMM = c.InternalDiameterMM,
                    ParticleSizeMicron = c.ParticleSizeMicron,
                    MaxPressureBar = c.MaxPressureBar,
                    ProtocolID = c.ProtocolID,
                    StatusID = c.StatusID,
                    TotalRuntimeHours = c.TotalRuntimeHours,
                    TotalInjections = c.TotalInjections,
                    InstalledOn = c.InstalledOn,
                    RetiredOn = c.RetiredOn,
                    CreatedOn = c.CreatedOn
                });
                _log.LogInformation("Successfully retrieved {ColumnCount} columns", items.Count);
                return ServiceResult<IEnumerable<ColumnMasterReadDto>>.Ok(dto);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error fetching columns");
                return ServiceResult<IEnumerable<ColumnMasterReadDto>>.Fail("An error occurred while retrieving columns.");
            }
        }

    // helper to extract more user-friendly messages from database update exceptions
    private string ParseDbUpdateException(DbUpdateException ex)
    {
        var baseEx = ex.GetBaseException();
        var message = baseEx?.Message ?? ex.Message;

        // SQL Server specific handling
        if (baseEx is SqlException sqlEx)
        {
            // Unique constraint / duplicate key
            if (sqlEx.Number == 2627 || sqlEx.Number == 2601 || message.IndexOf("UNIQUE", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (message.IndexOf("SerialNumber", StringComparison.OrdinalIgnoreCase) >= 0 || message.IndexOf("Serial", StringComparison.OrdinalIgnoreCase) >= 0)
                    return "Serial number already exists (unique constraint).";
                return "Unique constraint violation. Check unique fields (e.g., SerialNumber).";
            }

            // Foreign key violation
            if (sqlEx.Number == 547 || message.IndexOf("FOREIGN KEY", StringComparison.OrdinalIgnoreCase) >= 0 || message.IndexOf("REFERENCE", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (message.IndexOf("FK_Column_Protocol", StringComparison.OrdinalIgnoreCase) >= 0 || message.IndexOf("ProtocolID", StringComparison.OrdinalIgnoreCase) >= 0)
                    return "ProtocolID references a non-existing Protocol. Ensure the selected Protocol exists.";
                if (message.IndexOf("FK_Column_Status", StringComparison.OrdinalIgnoreCase) >= 0 || message.IndexOf("StatusID", StringComparison.OrdinalIgnoreCase) >= 0)
                    return "StatusID references a non-existing Status. Ensure the selected Status exists.";
                return "Foreign key constraint violation. Ensure related records exist.";
            }

            // Check constraint violation or other constraint
            if (message.IndexOf("CHECK constraint", StringComparison.OrdinalIgnoreCase) >= 0 || message.IndexOf("CHECK", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // try to map to known columns
                string[] positiveFields = new[] { "InternalDiameterMM", "LengthMM", "MaxPressureBar", "ParticleSizeMicron" };
                foreach (var f in positiveFields)
                {
                    if (message.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0)
                        return $"Value for {f} violates its check constraint. Ensure it is greater than zero.";
                }
                if (message.IndexOf("TotalRuntimeHours", StringComparison.OrdinalIgnoreCase) >= 0 || message.IndexOf("TotalInjections", StringComparison.OrdinalIgnoreCase) >= 0)
                    return "Total runtime hours and total injections must be zero or positive.";
                return "A check constraint was violated. Verify numeric ranges and required business rules.";
            }

            // String truncation / length issues
            if (message.IndexOf("String or binary data would be truncated", StringComparison.OrdinalIgnoreCase) >= 0 || message.IndexOf("truncated", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "One of the string fields is too long for the database column. Check max lengths for text fields (e.g., ColumnName, Manufacturer, SerialNumber).";
            }

            // Overflow or conversion
            if (message.IndexOf("overflow", StringComparison.OrdinalIgnoreCase) >= 0 || message.IndexOf("Arithmetic overflow", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "A numeric value is out of range for its database column. Check decimal precision/scale and integer ranges.";
            }
        }

        // Generic fallback provides the DB message for debugging but kept concise
        return "Database error while saving column: " + (message.Length > 200 ? message.Substring(0, 200) + "..." : message);
    }

    // Audit trail helper - log primary fields only for NEW action
    private async Task LogAuditCreateAsync(ColumnMaster entity)
    {
        try
        {
            // Get Status name for better readability
            var status = await _db.Status_Master.FirstOrDefaultAsync(s => s.StatusID == entity.StatusID);
            var statusName = status?.StatusName ?? $"Status ID {entity.StatusID}";

            // Get Protocol name if ProtocolID exists
            string? protocolName = null;
            if (entity.ProtocolID.HasValue && entity.ProtocolID > 0)
            {
                var protocol = await _db.Protocol.FirstOrDefaultAsync(p => p.ProtocolID == entity.ProtocolID);
                protocolName = protocol?.ProtocolName;
            }

            var newData = new
            {
                entity.ColumnName,
                entity.SerialNumber,
                Status = statusName,
                Protocol = protocolName
            };

            var audit = new AuditTrail
            {
                TableName = "Column_Master",
                RecordID = entity.ColumnID,
                OperationType = "NEW",
                OldData = null,
                NewData = JsonSerializer.Serialize(newData),
                ChangedBy = "System",
                ChangedOn = DateTime.UtcNow
            };

            await _auditService.CreateAsync(audit);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Failed to log audit trail for column creation {Id}", entity.ColumnID);
        }
    }

    // Audit trail helper - log all modified fields for UPDATE action
    private async Task LogAuditUpdateAsync(ColumnMaster oldEntity, ColumnMaster newEntity)
    {
        try
        {
            var changes = new Dictionary<string, object>();

            // Helper to resolve Status name from StatusID
            var getStatusName = new Func<int, Task<string>>(async (statusId) =>
            {
                var status = await _db.Status_Master.FirstOrDefaultAsync(s => s.StatusID == statusId);
                return status?.StatusName ?? $"Status ID {statusId}";
            });

            // Helper to resolve Protocol name from ProtocolID
            var getProtocolName = new Func<int?, Task<string?>>(async (protocolId) =>
            {
                if (!protocolId.HasValue || protocolId <= 0) return null;
                var protocol = await _db.Protocol.FirstOrDefaultAsync(p => p.ProtocolID == protocolId);
                return protocol?.ProtocolName;
            });

            // Check each field for changes
            if (oldEntity.ColumnName != newEntity.ColumnName)
                changes["ColumnName"] = new { Old = oldEntity.ColumnName, New = newEntity.ColumnName };
            if (oldEntity.Manufacturer != newEntity.Manufacturer)
                changes["Manufacturer"] = new { Old = oldEntity.Manufacturer, New = newEntity.Manufacturer };
            if (oldEntity.SerialNumber != newEntity.SerialNumber)
                changes["SerialNumber"] = new { Old = oldEntity.SerialNumber, New = newEntity.SerialNumber };
            if (oldEntity.LengthMM != newEntity.LengthMM)
                changes["LengthMM"] = new { Old = oldEntity.LengthMM, New = newEntity.LengthMM };
            if (oldEntity.InternalDiameterMM != newEntity.InternalDiameterMM)
                changes["InternalDiameterMM"] = new { Old = oldEntity.InternalDiameterMM, New = newEntity.InternalDiameterMM };
            if (oldEntity.ParticleSizeMicron != newEntity.ParticleSizeMicron)
                changes["ParticleSizeMicron"] = new { Old = oldEntity.ParticleSizeMicron, New = newEntity.ParticleSizeMicron };
            if (oldEntity.MaxPressureBar != newEntity.MaxPressureBar)
                changes["MaxPressureBar"] = new { Old = oldEntity.MaxPressureBar, New = newEntity.MaxPressureBar };

            if (oldEntity.ProtocolID != newEntity.ProtocolID)
            {
                var oldProtocolName = await getProtocolName(oldEntity.ProtocolID);
                var newProtocolName = await getProtocolName(newEntity.ProtocolID);
                changes["Protocol"] = new { Old = oldProtocolName ?? "(None)", New = newProtocolName ?? "(None)" };
            }

            if (oldEntity.StatusID != newEntity.StatusID)
            {
                var oldStatusName = await getStatusName(oldEntity.StatusID);
                var newStatusName = await getStatusName(newEntity.StatusID);
                changes["Status"] = new { Old = oldStatusName, New = newStatusName };
            }

            if (oldEntity.TotalRuntimeHours != newEntity.TotalRuntimeHours)
                changes["TotalRuntimeHours"] = new { Old = oldEntity.TotalRuntimeHours, New = newEntity.TotalRuntimeHours };
            if (oldEntity.TotalInjections != newEntity.TotalInjections)
                changes["TotalInjections"] = new { Old = oldEntity.TotalInjections, New = newEntity.TotalInjections };
            if (oldEntity.RetiredOn != newEntity.RetiredOn)
                changes["RetiredOn"] = new { Old = oldEntity.RetiredOn, New = newEntity.RetiredOn };

            // Only log if there are actual changes
            if (changes.Count > 0)
            {
                var audit = new AuditTrail
                {
                    TableName = "Column_Master",
                    RecordID = newEntity.ColumnID,
                    OperationType = "MODIFY",
                    OldData = null, // We track changes inline
                    NewData = JsonSerializer.Serialize(changes),
                    ChangedBy = "System",
                    ChangedOn = DateTime.UtcNow
                };

                await _auditService.CreateAsync(audit);
            }
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Failed to log audit trail for column update {Id}", newEntity.ColumnID);
        }
    }

        public async Task<ServiceResult<ColumnMasterReadDto>> GetByIdAsync(int id)
        {
            try
            {
                _log.LogDebug("Retrieving column with ID: {ColumnID}", id);
                var c = await _db.Column_Master.FindAsync(id);
                if (c == null)
                {
                    _log.LogWarning("Column with ID {ColumnID} not found", id);
                    return ServiceResult<ColumnMasterReadDto>.Fail("Column not found.");
                }

                var dto = new ColumnMasterReadDto
                {
                    ColumnID = c.ColumnID,
                    ColumnName = c.ColumnName,
                    Manufacturer = c.Manufacturer,
                    SerialNumber = c.SerialNumber,
                    LengthMM = c.LengthMM,
                    InternalDiameterMM = c.InternalDiameterMM,
                    ParticleSizeMicron = c.ParticleSizeMicron,
                    MaxPressureBar = c.MaxPressureBar,
                    ProtocolID = c.ProtocolID,
                    StatusID = c.StatusID,
                    TotalRuntimeHours = c.TotalRuntimeHours,
                    TotalInjections = c.TotalInjections,
                    InstalledOn = c.InstalledOn,
                    RetiredOn = c.RetiredOn,
                    CreatedOn = c.CreatedOn
                };
                _log.LogDebug("Successfully retrieved column: {ColumnName}", c.ColumnName);
                return ServiceResult<ColumnMasterReadDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error fetching column by id {Id}", id);
                return ServiceResult<ColumnMasterReadDto>.Fail("An error occurred while retrieving the column.");
            }
        }

        public async Task<ServiceResult<ColumnMasterReadDto>> CreateAsync(ColumnMasterCreateDto dto)
        {
            try
            {
                // basic validation beyond data annotations
                if (dto.LengthMM <= 0 || dto.InternalDiameterMM <= 0 || dto.MaxPressureBar <= 0 || dto.ParticleSizeMicron <= 0)
                    return ServiceResult<ColumnMasterReadDto>.Fail("Numeric values must be greater than zero for length, internal diameter, particle size and max pressure.");

                // Validate StatusID exists
                if (dto.StatusID <= 0)
                    return ServiceResult<ColumnMasterReadDto>.Fail("Please select a valid Status.");

                var statusExists = await _db.Status_Master.AnyAsync(s => s.StatusID == dto.StatusID);
                if (!statusExists)
                    return ServiceResult<ColumnMasterReadDto>.Fail($"StatusID {dto.StatusID} does not exist. Please create a status first or select a different one.");

                // Validate ProtocolID if provided
                if (dto.ProtocolID.HasValue && dto.ProtocolID > 0)
                {
                    var protocolExists = await _db.Protocol.AnyAsync(p => p.ProtocolID == dto.ProtocolID);
                    if (!protocolExists)
                        return ServiceResult<ColumnMasterReadDto>.Fail($"ProtocolID {dto.ProtocolID} does not exist. Please select a valid protocol or leave it blank.");
                }

                // check serial uniqueness
                var exists = await _db.Column_Master.AnyAsync(x => x.SerialNumber == dto.SerialNumber);
                if (exists)
                    return ServiceResult<ColumnMasterReadDto>.Fail("Serial number already exists.");

                var entity = new ColumnMaster
                {
                    ColumnName = dto.ColumnName,
                    Manufacturer = dto.Manufacturer,
                    SerialNumber = dto.SerialNumber,
                    LengthMM = dto.LengthMM,
                    InternalDiameterMM = dto.InternalDiameterMM,
                    ParticleSizeMicron = dto.ParticleSizeMicron,
                    MaxPressureBar = dto.MaxPressureBar,
                    ProtocolID = dto.ProtocolID,
                    StatusID = dto.StatusID,
                    TotalRuntimeHours = dto.TotalRuntimeHours,
                    TotalInjections = dto.TotalInjections,
                    InstalledOn = dto.InstalledOn,
                    RetiredOn = dto.RetiredOn,
                    CreatedOn = DateTime.UtcNow
                };

                _db.Column_Master.Add(entity);
                await _db.SaveChangesAsync();

                _log.LogInformation("Column created successfully: {ColumnName} (ID: {ColumnID}, SerialNumber: {SerialNumber})", 
                    entity.ColumnName, entity.ColumnID, entity.SerialNumber);

                // Log audit trail for creation (primary fields only)
                await LogAuditCreateAsync(entity);

                var read = new ColumnMasterReadDto
                {
                    ColumnID = entity.ColumnID,
                    ColumnName = entity.ColumnName,
                    Manufacturer = entity.Manufacturer,
                    SerialNumber = entity.SerialNumber,
                    LengthMM = entity.LengthMM,
                    InternalDiameterMM = entity.InternalDiameterMM,
                    ParticleSizeMicron = entity.ParticleSizeMicron,
                    MaxPressureBar = entity.MaxPressureBar,
                    ProtocolID = entity.ProtocolID,
                    StatusID = entity.StatusID,
                    TotalRuntimeHours = entity.TotalRuntimeHours,
                    TotalInjections = entity.TotalInjections,
                    InstalledOn = entity.InstalledOn,
                    RetiredOn = entity.RetiredOn,
                    CreatedOn = entity.CreatedOn
                };

                return ServiceResult<ColumnMasterReadDto>.Ok(read, "Column created successfully.");
            }
            catch (DbUpdateException dbEx)
            {
                _log.LogError(dbEx, "DB update error while creating column");
                var msg = ParseDbUpdateException(dbEx);
                return ServiceResult<ColumnMasterReadDto>.Fail(msg);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unexpected error while creating column");
                return ServiceResult<ColumnMasterReadDto>.Fail("An unexpected error occurred while creating the column.");
            }
        }

        public async Task<ServiceResult<ColumnMasterReadDto>> UpdateAsync(ColumnMasterUpdateDto dto)
        {
            try
            {
                var entity = await _db.Column_Master.FindAsync(dto.ColumnID);
                if (entity == null)
                    return ServiceResult<ColumnMasterReadDto>.Fail("Column not found.");

                // Prevent changing the column name after creation
                if (entity.ColumnName != dto.ColumnName)
                    return ServiceResult<ColumnMasterReadDto>.Fail("Column name cannot be modified after creation.");

                // Validate StatusID exists
                if (dto.StatusID <= 0)
                    return ServiceResult<ColumnMasterReadDto>.Fail("Please select a valid Status.");

                var statusExists = await _db.Status_Master.AnyAsync(s => s.StatusID == dto.StatusID);
                if (!statusExists)
                    return ServiceResult<ColumnMasterReadDto>.Fail($"StatusID {dto.StatusID} does not exist. Please select a valid status.");

                // Validate ProtocolID if provided
                if (dto.ProtocolID.HasValue && dto.ProtocolID > 0)
                {
                    var protocolExists = await _db.Protocol.AnyAsync(p => p.ProtocolID == dto.ProtocolID);
                    if (!protocolExists)
                        return ServiceResult<ColumnMasterReadDto>.Fail($"ProtocolID {dto.ProtocolID} does not exist. Please select a valid protocol or leave it blank.");
                }

                // check serial uniqueness if changed
                if (entity.SerialNumber != dto.SerialNumber)
                {
                    var exists = await _db.Column_Master.AnyAsync(x => x.SerialNumber == dto.SerialNumber && x.ColumnID != dto.ColumnID);
                    if (exists)
                        return ServiceResult<ColumnMasterReadDto>.Fail("Serial number already exists.");
                }

                entity.ColumnName = dto.ColumnName;
                entity.Manufacturer = dto.Manufacturer;
                entity.SerialNumber = dto.SerialNumber;
                entity.LengthMM = dto.LengthMM;
                entity.InternalDiameterMM = dto.InternalDiameterMM;
                entity.ParticleSizeMicron = dto.ParticleSizeMicron;
                entity.MaxPressureBar = dto.MaxPressureBar;
                entity.ProtocolID = dto.ProtocolID;
                entity.StatusID = dto.StatusID;
                entity.TotalRuntimeHours = dto.TotalRuntimeHours;
                entity.TotalInjections = dto.TotalInjections;
                entity.InstalledOn = dto.InstalledOn;
                entity.RetiredOn = dto.RetiredOn;

                // Store the old entity data before update
                var oldEntity = await _db.Column_Master.AsNoTracking().FirstOrDefaultAsync(x => x.ColumnID == dto.ColumnID);

                await _db.SaveChangesAsync();

                _log.LogInformation("Column updated successfully: {ColumnName} (ID: {ColumnID})", 
                    entity.ColumnName, entity.ColumnID);

                // Log audit trail for modification (all changed fields)
                if (oldEntity != null)
                    await LogAuditUpdateAsync(oldEntity, entity);

                var read = new ColumnMasterReadDto
                {
                    ColumnID = entity.ColumnID,
                    ColumnName = entity.ColumnName,
                    Manufacturer = entity.Manufacturer,
                    SerialNumber = entity.SerialNumber,
                    LengthMM = entity.LengthMM,
                    InternalDiameterMM = entity.InternalDiameterMM,
                    ParticleSizeMicron = entity.ParticleSizeMicron,
                    MaxPressureBar = entity.MaxPressureBar,
                    ProtocolID = entity.ProtocolID,
                    StatusID = entity.StatusID,
                    TotalRuntimeHours = entity.TotalRuntimeHours,
                    TotalInjections = entity.TotalInjections,
                    InstalledOn = entity.InstalledOn,
                    RetiredOn = entity.RetiredOn,
                    CreatedOn = entity.CreatedOn
                };

                return ServiceResult<ColumnMasterReadDto>.Ok(read, "Column updated successfully.");
            }
            catch (DbUpdateException dbEx)
            {
                _log.LogError(dbEx, "DB update error while updating column {Id}", dto.ColumnID);
                var msg = ParseDbUpdateException(dbEx);
                return ServiceResult<ColumnMasterReadDto>.Fail(msg);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unexpected error while updating column {Id}", dto.ColumnID);
                return ServiceResult<ColumnMasterReadDto>.Fail("An unexpected error occurred while updating the column.");
            }
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            try
            {
                _log.LogInformation("Attempting to delete column with ID: {ColumnID}", id);
                var entity = await _db.Column_Master.FindAsync(id);
                if (entity == null)
                {
                    _log.LogWarning("Column with ID {ColumnID} not found for deletion", id);
                    return ServiceResult<bool>.Fail("Column not found.");
                }

                // Check referential integrity: existing usages or maintenance
                var hasUsage = await _db.Column_Usage_Log.AnyAsync(u => u.ColumnID == id);
                var hasMaintenance = await _db.Column_Maintenance_Log.AnyAsync(m => m.ColumnID == id);
                if (hasUsage || hasMaintenance)
                {
                    _log.LogWarning("Cannot delete column {ColumnName} (ID: {ColumnID}) - has existing usage or maintenance records", 
                        entity.ColumnName, id);
                    return ServiceResult<bool>.Fail("Cannot delete column with existing usage or maintenance records.");
                }

                _db.Column_Master.Remove(entity);
                await _db.SaveChangesAsync();
                _log.LogInformation("Column deleted successfully: {ColumnName} (ID: {ColumnID})", entity.ColumnName, id);
                return ServiceResult<bool>.Ok(true, "Column deleted.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error deleting column {Id}", id);
                return ServiceResult<bool>.Fail("An error occurred while deleting the column.");
            }
        }
    }
}
