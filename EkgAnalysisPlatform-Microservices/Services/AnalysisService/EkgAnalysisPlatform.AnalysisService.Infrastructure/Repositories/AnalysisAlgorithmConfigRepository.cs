using EkgAnalysisPlatform.AnalysisService.Domain.Models;
using EkgAnalysisPlatform.AnalysisService.Domain.Repositories;
using EkgAnalysisPlatform.AnalysisService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EkgAnalysisPlatform.AnalysisService.Infrastructure.Repositories
{
    public class AnalysisAlgorithmConfigRepository : IAnalysisAlgorithmConfigRepository
    {
        private readonly AnalysisDbContext _context;

        public AnalysisAlgorithmConfigRepository(AnalysisDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AnalysisAlgorithmConfig>> GetAllAsync()
        {
            return await _context.AlgorithmConfigs.ToListAsync();
        }

        public async Task<AnalysisAlgorithmConfig?> GetByIdAsync(int id)
        {
            return await _context.AlgorithmConfigs.FindAsync(id);
        }

        public async Task<AnalysisAlgorithmConfig?> GetActiveAlgorithmAsync(string algorithmName)
        {
            return await _context.AlgorithmConfigs
                .Where(c => c.AlgorithmName == algorithmName && c.IsActive)
                .FirstOrDefaultAsync();
        }

        public async Task<int> AddAsync(AnalysisAlgorithmConfig config)
        {
            _context.AlgorithmConfigs.Add(config);
            await _context.SaveChangesAsync();
            return config.Id;
        }

        public async Task UpdateAsync(AnalysisAlgorithmConfig config)
        {
            _context.Entry(config).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var config = await _context.AlgorithmConfigs.FindAsync(id);
            if (config != null)
            {
                _context.AlgorithmConfigs.Remove(config);
                await _context.SaveChangesAsync();
            }
        }
    }
}