using COLManager.Web.Common;
using COLManager.Web.Data;
using COLManager.Web.DTOs;
using COLManager.Web.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace COLManager.Web.Services
{
    public class ProtocolService : IProtocolService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ProtocolService> _log;
        private readonly IAuditService _auditService;

        public ProtocolService(ApplicationDbContext db, ILogger<ProtocolService> log, IAuditService auditService)
        {
            _db = db;
            _log = log;
            _auditService = auditService;
        }

        // Audit trail helper - log primary fields only for NEW action
        private async Task LogAuditCreateAsync(Protocol entity)
        {
            try
            {
                var newData = new
                {
                    entity.ProtocolName,
                    entity.MaxAllowedPressureBar
                };

                var audit = new AuditTrail
                {
                    TableName = "Protocol",
                    RecordID = entity.ProtocolID,
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
                _log.LogWarning(ex, "Failed to log audit trail for protocol creation {Id}", entity.ProtocolID);
            }
        }

        // Audit trail helper - log all modified fields for UPDATE action
        private async Task LogAuditUpdateAsync(Protocol oldEntity, Protocol newEntity)
        {
            try
            {
                var changes = new Dictionary<string, object>();

                // Check each field for changes
                if (oldEntity.ProtocolName != newEntity.ProtocolName)
                    changes["ProtocolName"] = new { Old = oldEntity.ProtocolName, New = newEntity.ProtocolName };
                if (oldEntity.Description != newEntity.Description)
                    changes["Description"] = new { Old = oldEntity.Description, New = newEntity.Description };
                if (oldEntity.MaxAllowedPressureBar != newEntity.MaxAllowedPressureBar)
                    changes["MaxAllowedPressureBar"] = new { Old = oldEntity.MaxAllowedPressureBar, New = newEntity.MaxAllowedPressureBar };
                if (oldEntity.MaxAllowedUsageHours != newEntity.MaxAllowedUsageHours)
                    changes["MaxAllowedUsageHours"] = new { Old = oldEntity.MaxAllowedUsageHours, New = newEntity.MaxAllowedUsageHours };
                if (oldEntity.MaxAllowedInjections != newEntity.MaxAllowedInjections)
                    changes["MaxAllowedInjections"] = new { Old = oldEntity.MaxAllowedInjections, New = newEntity.MaxAllowedInjections };
                if (oldEntity.OperatingTemperatureC != newEntity.OperatingTemperatureC)
                    changes["OperatingTemperatureC"] = new { Old = oldEntity.OperatingTemperatureC, New = newEntity.OperatingTemperatureC };
                if (oldEntity.IsActive != newEntity.IsActive)
                    changes["IsActive"] = new { Old = oldEntity.IsActive, New = newEntity.IsActive };

                // Only log if there are actual changes
                if (changes.Count > 0)
                {
                    var audit = new AuditTrail
                    {
                        TableName = "Protocol",
                        RecordID = newEntity.ProtocolID,
                        OperationType = "MODIFY",
                        OldData = null,
                        NewData = JsonSerializer.Serialize(changes),
                        ChangedBy = "System",
                        ChangedOn = DateTime.UtcNow
                    };

                    await _auditService.CreateAsync(audit);
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Failed to log audit trail for protocol update {Id}", newEntity.ProtocolID);
            }
        }

        public async Task<ServiceResult<ProtocolReadDto>> CreateAsync(ProtocolCreateDto dto)
        {
            try
            {
                if (dto.MaxAllowedPressureBar <= 0)
                    return ServiceResult<ProtocolReadDto>.Fail("Max allowed pressure must be greater than zero.");

                var entity = new Protocol
                {
                    ProtocolName = dto.ProtocolName,
                    Description = dto.Description,
                    MaxAllowedPressureBar = dto.MaxAllowedPressureBar,
                    MaxAllowedUsageHours = dto.MaxAllowedUsageHours,
                    MaxAllowedInjections = dto.MaxAllowedInjections,
                    OperatingTemperatureC = dto.OperatingTemperatureC,
                    CreatedOn = DateTime.UtcNow,
                    IsActive = true
                };

                _db.Protocol.Add(entity);
                await _db.SaveChangesAsync();

                _log.LogInformation("Protocol created successfully: {ProtocolName} (ID: {ProtocolID}, MaxPressure: {MaxPressure}bar)", 
                    entity.ProtocolName, entity.ProtocolID, entity.MaxAllowedPressureBar);

                // Log audit trail for creation (primary fields only)
                await LogAuditCreateAsync(entity);

                var read = new ProtocolReadDto
                {
                    ProtocolID = entity.ProtocolID,
                    ProtocolName = entity.ProtocolName,
                    Description = entity.Description,
                    MaxAllowedPressureBar = entity.MaxAllowedPressureBar,
                    MaxAllowedUsageHours = entity.MaxAllowedUsageHours,
                    MaxAllowedInjections = entity.MaxAllowedInjections,
                    OperatingTemperatureC = entity.OperatingTemperatureC,
                    CreatedOn = entity.CreatedOn,
                    IsActive = entity.IsActive
                };

                return ServiceResult<ProtocolReadDto>.Ok(read, "Protocol created.");
            }
            catch (DbUpdateException dbEx)
            {
                _log.LogError(dbEx, "DB error creating protocol");
                return ServiceResult<ProtocolReadDto>.Fail("Database error while creating protocol.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unexpected error creating protocol");
                return ServiceResult<ProtocolReadDto>.Fail("An unexpected error occurred while creating protocol.");
            }
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            try
            {
                _log.LogInformation("Attempting to delete protocol with ID: {ProtocolID}", id);
                var entity = await _db.Protocol.FindAsync(id);
                if (entity == null)
                {
                    _log.LogWarning("Protocol with ID {ProtocolID} not found for deletion", id);
                    return ServiceResult<bool>.Fail("Protocol not found.");
                }

                var used = await _db.Column_Master.AnyAsync(c => c.ProtocolID == id);
                if (used)
                {
                    _log.LogWarning("Cannot delete protocol {ProtocolName} (ID: {ProtocolID}) - in use by columns", 
                        entity.ProtocolName, id);
                    return ServiceResult<bool>.Fail("Cannot delete protocol in use by columns.");
                }

                _db.Protocol.Remove(entity);
                await _db.SaveChangesAsync();
                _log.LogInformation("Protocol deleted successfully: {ProtocolName} (ID: {ProtocolID})", entity.ProtocolName, id);
                return ServiceResult<bool>.Ok(true, "Protocol deleted.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error deleting protocol {Id}", id);
                return ServiceResult<bool>.Fail("An error occurred while deleting protocol.");
            }
        }

        public async Task<ServiceResult<IEnumerable<ProtocolReadDto>>> GetAllAsync()
        {
            try
            {
                _log.LogInformation("Retrieving all protocols from database");
                var list = await _db.Protocol.OrderBy(p => p.ProtocolName).ToListAsync();
                var dto = list.Select(e => new ProtocolReadDto
                {
                    ProtocolID = e.ProtocolID,
                    ProtocolName = e.ProtocolName,
                    Description = e.Description,
                    MaxAllowedPressureBar = e.MaxAllowedPressureBar,
                    MaxAllowedUsageHours = e.MaxAllowedUsageHours,
                    MaxAllowedInjections = e.MaxAllowedInjections,
                    OperatingTemperatureC = e.OperatingTemperatureC,
                    CreatedOn = e.CreatedOn,
                    IsActive = e.IsActive
                });
                _log.LogInformation("Successfully retrieved {ProtocolCount} protocols", list.Count);
                return ServiceResult<IEnumerable<ProtocolReadDto>>.Ok(dto);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error fetching protocols");
                return ServiceResult<IEnumerable<ProtocolReadDto>>.Fail("An error occurred while retrieving protocols.");
            }
        }

        public async Task<ServiceResult<ProtocolReadDto>> GetByIdAsync(int id)
        {
            try
            {
                var e = await _db.Protocol.FindAsync(id);
                if (e == null) return ServiceResult<ProtocolReadDto>.Fail("Protocol not found.");
                var dto = new ProtocolReadDto
                {
                    ProtocolID = e.ProtocolID,
                    ProtocolName = e.ProtocolName,
                    Description = e.Description,
                    MaxAllowedPressureBar = e.MaxAllowedPressureBar,
                    MaxAllowedUsageHours = e.MaxAllowedUsageHours,
                    MaxAllowedInjections = e.MaxAllowedInjections,
                    OperatingTemperatureC = e.OperatingTemperatureC,
                    CreatedOn = e.CreatedOn,
                    IsActive = e.IsActive
                };
                return ServiceResult<ProtocolReadDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error fetching protocol {Id}", id);
                return ServiceResult<ProtocolReadDto>.Fail("An error occurred while retrieving protocol.");
            }
        }

        public async Task<ServiceResult<ProtocolReadDto>> UpdateAsync(int id, ProtocolCreateDto dto)
        {
            try
            {
                var e = await _db.Protocol.FindAsync(id);
                if (e == null) return ServiceResult<ProtocolReadDto>.Fail("Protocol not found.");

                // Prevent changing the protocol name after creation
                if (e.ProtocolName != dto.ProtocolName)
                    return ServiceResult<ProtocolReadDto>.Fail("Protocol name cannot be modified after creation.");

                e.ProtocolName = dto.ProtocolName;
                e.Description = dto.Description;
                e.MaxAllowedPressureBar = dto.MaxAllowedPressureBar;
                e.MaxAllowedUsageHours = dto.MaxAllowedUsageHours;
                e.MaxAllowedInjections = dto.MaxAllowedInjections;
                e.OperatingTemperatureC = dto.OperatingTemperatureC;

                // Store the old entity data before update
                var oldEntity = await _db.Protocol.AsNoTracking().FirstOrDefaultAsync(x => x.ProtocolID == id);

                await _db.SaveChangesAsync();

                // Log audit trail for modification (all changed fields)
                if (oldEntity != null)
                    await LogAuditUpdateAsync(oldEntity, e);

                var read = new ProtocolReadDto
                {
                    ProtocolID = e.ProtocolID,
                    ProtocolName = e.ProtocolName,
                    Description = e.Description,
                    MaxAllowedPressureBar = e.MaxAllowedPressureBar,
                    MaxAllowedUsageHours = e.MaxAllowedUsageHours,
                    MaxAllowedInjections = e.MaxAllowedInjections,
                    OperatingTemperatureC = e.OperatingTemperatureC,
                    CreatedOn = e.CreatedOn,
                    IsActive = e.IsActive
                };

                return ServiceResult<ProtocolReadDto>.Ok(read, "Protocol updated.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error updating protocol {Id}", id);
                return ServiceResult<ProtocolReadDto>.Fail("An error occurred while updating protocol.");
            }
        }
    }
}
