using COLManager.Web.Common;
using COLManager.Web.Entities;

namespace COLManager.Web.Services
{
    public interface IStatusService
    {
        Task<ServiceResult<IEnumerable<StatusMaster>>> GetAllAsync();
        Task<ServiceResult<StatusMaster>> GetByIdAsync(int id);
        Task<ServiceResult<StatusMaster>> CreateAsync(StatusMaster s);
        Task<ServiceResult<StatusMaster>> UpdateAsync(StatusMaster s);
        Task<ServiceResult<bool>> DeleteAsync(int id);
    }
}
