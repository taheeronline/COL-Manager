using COLManager.Web.Common;
using COLManager.Web.DTOs;

namespace COLManager.Web.Services
{
    public interface IUsageService
    {
        Task<ServiceResult<bool>> CreateCheckoutAsync(ColumnCheckoutCreateDto dto);
        Task<ServiceResult<bool>> CheckinAsync(ColumnCheckinDto dto);

        Task<List<ColumnCheckoutReadDto>> GetCheckoutsAsync();
        Task<List<ColumnCheckoutReadDto>> GetCheckedOutOnlyAsync();

        Task<List<ColumnUsageLifecycleReadDto>> GetLifecycleAsync();
    }
}