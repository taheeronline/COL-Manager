using COLManager.Web.Common;
using COLManager.Web.Entities;

namespace COLManager.Web.Services
{
    public interface IAuditService
    {
        Task<ServiceResult<IEnumerable<AuditTrail>>> GetAllAsync();
        Task<ServiceResult<AuditTrail>> GetByIdAsync(int id);
        Task<ServiceResult<AuditTrail>> CreateAsync(AuditTrail a);
        Task<ServiceResult<IEnumerable<AuditTrail>>> GetByTableAndRecordAsync(string tableName, int recordId);
    }
}
