using EkgAnalysisPlatform.BatchProcessingService.Domain.Models;

namespace EkgAnalysisPlatform.BatchProcessingService.Domain.Repositories
{
    public interface IBatchJobItemRepository
    {
        Task<IEnumerable<BatchJobItem>> GetAllByBatchJobIdAsync(string batchJobId);
        Task<BatchJobItem?> GetByIdAsync(int id);
        Task<IEnumerable<BatchJobItem>> GetByStatusAsync(string batchJobId, BatchJobItemStatus status);
        Task<int> AddAsync(BatchJobItem item);
        Task AddRangeAsync(IEnumerable<BatchJobItem> items);
        Task UpdateAsync(BatchJobItem item);
        Task UpdateStatusAsync(int id, BatchJobItemStatus status);
        Task<int> GetCountByStatusAsync(string batchJobId, BatchJobItemStatus status);
        Task<IEnumerable<BatchJobItem>> GetItemsForRetryAsync();
    }
}