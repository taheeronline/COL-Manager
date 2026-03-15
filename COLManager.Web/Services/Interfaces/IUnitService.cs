using COLManager.Web.Common;
using COLManager.Web.Entities;

namespace COLManager.Web.Services
{
    public interface IUnitService
    {
        Task<ServiceResult<IEnumerable<UnitMaster>>> GetAllAsync(int? measurementTypeId = null);
        Task<ServiceResult<UnitMaster>> GetByIdAsync(int id);
        Task<ServiceResult<UnitMaster>> CreateAsync(UnitMaster u);
        Task<ServiceResult<UnitMaster>> UpdateAsync(UnitMaster u);
        Task<ServiceResult<bool>> DeleteAsync(int id);
    }
}
