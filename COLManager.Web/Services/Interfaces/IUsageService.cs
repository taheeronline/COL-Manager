using COLManager.Web.Common;
using COLManager.Web.DTOs;

namespace COLManager.Web.Services
{
    public interface IUsageService
    {
        Task<ServiceResult<IEnumerable<ColumnUsageReadDto>>> GetAllAsync(int? columnId = null);
        Task<ServiceResult<ColumnUsageReadDto>> GetByIdAsync(int id);
        Task<ServiceResult<ColumnUsageReadDto>> CreateAsync(ColumnUsageCreateDto dto);
        Task<ServiceResult<bool>> DeleteAsync(int id);
    }
}
