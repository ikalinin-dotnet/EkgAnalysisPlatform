namespace EkgAnalysisPlatform.BatchProcessingService.Domain.Repositories
{
    public interface IBatchJobRepository
    {
        Task<IEnumerable<BatchJob>> GetAllAsync();
        Task<BatchJob?> GetByIdAsync(int id);
        Task<BatchJob?> GetByJobIdAsync(string jobId);
        Task<IEnumerable<BatchJob>> GetByStatusAsync(BatchJobStatus status);
        Task<IEnumerable<BatchJob>> GetActiveJobsAsync();
        Task<int> AddAsync(BatchJob job);
        Task UpdateAsync(BatchJob job);
        Task UpdateStatusAsync(string jobId, BatchJobStatus status);
        Task DeleteAsync(int id);
    }
}