using COLManager.Web.Common;
using COLManager.Web.Entities;

namespace COLManager.Web.Services
{
    public interface IMeasurementService
    {
        Task<ServiceResult<IEnumerable<MeasurementType>>> GetAllAsync();
        Task<ServiceResult<MeasurementType>> GetByIdAsync(int id);
        Task<ServiceResult<MeasurementType>> CreateAsync(MeasurementType m);
        Task<ServiceResult<MeasurementType>> UpdateAsync(MeasurementType m);
        Task<ServiceResult<bool>> DeleteAsync(int id);
    }
}
