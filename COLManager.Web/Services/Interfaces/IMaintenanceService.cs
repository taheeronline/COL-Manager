using COLManager.Web.Common;
using COLManager.Web.Entities;

namespace COLManager.Web.Services
{
    public interface IMaintenanceService
    {
        Task<ServiceResult<IEnumerable<ColumnMaintenanceLog>>> GetByColumnAsync(int columnId);
        Task<ServiceResult<ColumnMaintenanceLog>> CreateAsync(ColumnMaintenanceLog m);
    }
}
