namespace EkgAnalysisPlatform.AnalysisService.Domain.Repositories
{
    public interface IAnalysisRequestRepository
    {
        Task<IEnumerable<AnalysisRequest>> GetAllAsync();
        Task<AnalysisRequest?> GetByIdAsync(int id);
        Task<IEnumerable<AnalysisRequest>> GetByStatusAsync(AnalysisStatus status);
        Task<IEnumerable<AnalysisRequest>> GetPendingRequestsAsync();
        Task<int> AddAsync(AnalysisRequest request);
        Task UpdateAsync(AnalysisRequest request);
        Task UpdateStatusAsync(int id, AnalysisStatus status);
        Task DeleteAsync(int id);
    }
}