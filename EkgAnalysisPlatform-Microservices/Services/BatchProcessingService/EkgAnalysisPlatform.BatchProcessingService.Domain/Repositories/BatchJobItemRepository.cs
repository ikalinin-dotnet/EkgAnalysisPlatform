using EkgAnalysisPlatform.BatchProcessingService.Domain.Models;
using EkgAnalysisPlatform.BatchProcessingService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EkgAnalysisPlatform.BatchProcessingService.Infrastructure.Repositories
{
    public class BatchJobItemRepository : IBatchJobItemRepository
    {
        private readonly BatchDbContext _context;

        public BatchJobItemRepository(BatchDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BatchJobItem>> GetAllByBatchJobIdAsync(string batchJobId)
        {
            return await _context.BatchJobItems
                .Where(i => i.BatchJobId == batchJobId)
                .ToListAsync();
        }

        public async Task<BatchJobItem?> GetByIdAsync(int id)
        {
            return await _context.BatchJobItems.FindAsync(id);
        }

        public async Task<IEnumerable<BatchJobItem>> GetByStatusAsync(string batchJobId, BatchJobItemStatus status)
        {
            return await _context.BatchJobItems
                .Where(i => i.BatchJobId == batchJobId && i.Status == status)
                .ToListAsync();
        }

        public async Task<int> AddAsync(BatchJobItem item)
        {
            _context.BatchJobItems.Add(item);
            await _context.SaveChangesAsync();
            return item.Id;
        }

        public async Task AddRangeAsync(IEnumerable<BatchJobItem> items)
        {
            _context.BatchJobItems.AddRange(items);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(BatchJobItem item)
        {
            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(int id, BatchJobItemStatus status)
        {
            var item = await _context.BatchJobItems.FindAsync(id);
            if (item != null)
            {
                item.Status = status;
                if (status == BatchJobItemStatus.Completed || status == BatchJobItemStatus.Failed)
                {
                    item.ProcessedAt = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetCountByStatusAsync(string batchJobId, BatchJobItemStatus status)
        {
            return await _context.BatchJobItems
                .CountAsync(i => i.BatchJobId == batchJobId && i.Status == status);
        }

        public async Task<IEnumerable<BatchJobItem>> GetItemsForRetryAsync()
        {
            return await _context.BatchJobItems
                .Where(i => i.Status == BatchJobItemStatus.Failed 
                        && i.NextRetryAt.HasValue 
                        && i.NextRetryAt <= DateTime.UtcNow)
                .ToListAsync();
        }
    }
}