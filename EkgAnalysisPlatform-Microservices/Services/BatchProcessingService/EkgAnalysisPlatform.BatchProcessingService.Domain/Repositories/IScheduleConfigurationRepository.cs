using EkgAnalysisPlatform.BatchProcessingService.Domain.Models;

namespace EkgAnalysisPlatform.BatchProcessingService.Domain.Repositories
{
    public interface IScheduleConfigurationRepository
    {
        Task<IEnumerable<ScheduleConfiguration>> GetAllAsync();
        Task<ScheduleConfiguration?> GetByIdAsync(int id);
        Task<ScheduleConfiguration?> GetByJobTypeAsync(string jobType);
        Task<IEnumerable<ScheduleConfiguration>> GetEnabledConfigurationsAsync();
        Task<int> AddAsync(ScheduleConfiguration config);
        Task UpdateAsync(ScheduleConfiguration config);
        Task DeleteAsync(int id);
    }
}