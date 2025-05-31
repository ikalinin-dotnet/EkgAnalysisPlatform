using EkgAnalysisPlatform.AnalysisService.Domain.Models;

namespace EkgAnalysisPlatform.AnalysisService.Domain.Repositories
{
    public interface IAnalysisAlgorithmConfigRepository
    {
        Task<IEnumerable<AnalysisAlgorithmConfig>> GetAllAsync();
        Task<AnalysisAlgorithmConfig?> GetByIdAsync(int id);
        Task<AnalysisAlgorithmConfig?> GetActiveAlgorithmAsync(string algorithmName);
        Task<int> AddAsync(AnalysisAlgorithmConfig config);
        Task UpdateAsync(AnalysisAlgorithmConfig config);
        Task DeleteAsync(int id);
    }
}