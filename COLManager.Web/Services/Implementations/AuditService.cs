using COLManager.Web.Common;
using COLManager.Web.Data;
using COLManager.Web.Entities;
using Microsoft.EntityFrameworkCore;

namespace COLManager.Web.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<AuditService> _log;

        public AuditService(ApplicationDbContext db, ILogger<AuditService> log)
        {
            _db = db;
            _log = log;
        }

        public async Task<ServiceResult<AuditTrail>> CreateAsync(AuditTrail a)
        {
            try
            {
                a.ChangedOn = a.ChangedOn == default ? DateTime.UtcNow : a.ChangedOn;
                _db.Audit_Trail.Add(a);
                await _db.SaveChangesAsync();
                _log.LogDebug("Audit entry created: Table={TableName}, Record={RecordID}, Operation={OperationType}, ChangedBy={ChangedBy}", 
                    a.TableName, a.RecordID, a.OperationType, a.ChangedBy);
                return ServiceResult<AuditTrail>.Ok(a, "Audit entry created.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error creating audit entry for Table={TableName}, Record={RecordID}", a.TableName, a.RecordID);
                return ServiceResult<AuditTrail>.Fail("An error occurred while creating audit entry.");
            }
        }

        public async Task<ServiceResult<IEnumerable<AuditTrail>>> GetAllAsync()
        {
            try
            {
                var list = await _db.Audit_Trail.OrderByDescending(a => a.ChangedOn).ToListAsync();
                return ServiceResult<IEnumerable<AuditTrail>>.Ok(list);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error fetching audits");
                return ServiceResult<IEnumerable<AuditTrail>>.Fail("An error occurred while retrieving audit entries.");
            }
        }

        public async Task<ServiceResult<AuditTrail>> GetByIdAsync(int id)
        {
            try
            {
                var e = await _db.Audit_Trail.FindAsync(id);
                if (e == null) return ServiceResult<AuditTrail>.Fail("Audit entry not found.");
                return ServiceResult<AuditTrail>.Ok(e);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error fetching audit {Id}", id);
                return ServiceResult<AuditTrail>.Fail("An error occurred while retrieving audit entry.");
            }
        }

        /// <summary>
        /// Get audit trail entries for a specific record (by table name and record ID)
        /// </summary>
        public async Task<ServiceResult<IEnumerable<AuditTrail>>> GetByTableAndRecordAsync(string tableName, int recordId)
        {
            try
            {
                var list = await _db.Audit_Trail
                    .Where(a => a.TableName == tableName && a.RecordID == recordId)
                    .OrderByDescending(a => a.ChangedOn)
                    .ToListAsync();
                return ServiceResult<IEnumerable<AuditTrail>>.Ok(list);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error fetching audit trail for {TableName} ID {RecordId}", tableName, recordId);
                return ServiceResult<IEnumerable<AuditTrail>>.Fail("An error occurred while retrieving audit trail.");
            }
        }
    }
}
