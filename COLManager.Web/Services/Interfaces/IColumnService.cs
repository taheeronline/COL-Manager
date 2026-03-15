using COLManager.Web.Common;
using COLManager.Web.DTOs;

namespace COLManager.Web.Services
{
    public interface IColumnService
    {
        Task<ServiceResult<IEnumerable<ColumnMasterReadDto>>> GetAllAsync();
        Task<ServiceResult<ColumnMasterReadDto>> GetByIdAsync(int id);
        Task<ServiceResult<ColumnMasterReadDto>> CreateAsync(ColumnMasterCreateDto dto);
        Task<ServiceResult<ColumnMasterReadDto>> UpdateAsync(ColumnMasterUpdateDto dto);
        Task<ServiceResult<bool>> DeleteAsync(int id);
    }
}
