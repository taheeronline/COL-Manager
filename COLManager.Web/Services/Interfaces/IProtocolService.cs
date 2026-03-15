using COLManager.Web.Common;
using COLManager.Web.DTOs;

namespace COLManager.Web.Services
{
    public interface IProtocolService
    {
        Task<ServiceResult<IEnumerable<ProtocolReadDto>>> GetAllAsync();
        Task<ServiceResult<ProtocolReadDto>> GetByIdAsync(int id);
        Task<ServiceResult<ProtocolReadDto>> CreateAsync(ProtocolCreateDto dto);
        Task<ServiceResult<ProtocolReadDto>> UpdateAsync(int id, ProtocolCreateDto dto);
        Task<ServiceResult<bool>> DeleteAsync(int id);
    }
}
