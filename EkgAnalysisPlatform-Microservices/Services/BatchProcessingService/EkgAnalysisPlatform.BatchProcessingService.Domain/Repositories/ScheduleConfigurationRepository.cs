using EkgAnalysisPlatform.BatchProcessingService.Domain.Models;
using EkgAnalysisPlatform.BatchProcessingService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EkgAnalysisPlatform.BatchProcessingService.Infrastructure.Repositories
{
    public class ScheduleConfigurationRepository : IScheduleConfigurationRepository
    {
        private readonly BatchDbContext _context;

        public ScheduleConfigurationRepository(BatchDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ScheduleConfiguration>> GetAllAsync()
        {
            return await _context.ScheduleConfigurations.ToListAsync();
        }

        public async Task<ScheduleConfiguration?> GetByIdAsync(int id)
        {
            return await _context.ScheduleConfigurations.FindAsync(id);
        }

        public async Task<ScheduleConfiguration?> GetByJobTypeAsync(string jobType)
        {
            return await _context.ScheduleConfigurations
                .FirstOrDefaultAsync(c => c.JobType == jobType);
        }

        public async Task<IEnumerable<ScheduleConfiguration>> GetEnabledConfigurationsAsync()
        {
            return await _context.ScheduleConfigurations
                .Where(c => c.IsEnabled)
                .ToListAsync();
        }

        public async Task<int> AddAsync(ScheduleConfiguration config)
        {
            _context.ScheduleConfigurations.Add(config);
            await _context.SaveChangesAsync();
            return config.Id;
        }

        public async Task UpdateAsync(ScheduleConfiguration config)
        {
            _context.Entry(config).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var config = await _context.ScheduleConfigurations.FindAsync(id);
            if (config != null)
            {
                _context.ScheduleConfigurations.Remove(config);
                await _context.SaveChangesAsync();
            }
        }
    }
}