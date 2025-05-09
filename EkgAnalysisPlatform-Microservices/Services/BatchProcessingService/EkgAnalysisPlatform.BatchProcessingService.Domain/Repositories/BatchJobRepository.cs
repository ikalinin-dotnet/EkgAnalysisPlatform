using EkgAnalysisPlatform.BatchProcessingService.Domain.Models;
using EkgAnalysisPlatform.BatchProcessingService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EkgAnalysisPlatform.BatchProcessingService.Infrastructure.Repositories
{
    public class BatchJobRepository : IBatchJobRepository
    {
        private readonly BatchDbContext _context;

        public BatchJobRepository(BatchDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BatchJob>> GetAllAsync()
        {
            return await _context.BatchJobs.ToListAsync();
        }

        public async Task<BatchJob?> GetByIdAsync(int id)
        {
            return await _context.BatchJobs.FindAsync(id);
        }

        public async Task<BatchJob?> GetByJobIdAsync(string jobId)
        {
            return await _context.BatchJobs
                .FirstOrDefaultAsync(j => j.JobId == jobId);
        }

        public async Task<IEnumerable<BatchJob>> GetByStatusAsync(BatchJobStatus status)
        {
            return await _context.BatchJobs
                .Where(j => j.Status == status)
                .ToListAsync();
        }

        public async Task<IEnumerable<BatchJob>> GetActiveJobsAsync()
        {
            return await _context.BatchJobs
                .Where(j => j.Status == BatchJobStatus.Running || j.Status == BatchJobStatus.Pending)
                .ToListAsync();
        }

        public async Task<int> AddAsync(BatchJob job)
        {
            _context.BatchJobs.Add(job);
            await _context.SaveChangesAsync();
            return job.Id;
        }

        public async Task UpdateAsync(BatchJob job)
        {
            _context.Entry(job).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(string jobId, BatchJobStatus status)
        {
            var job = await _context.BatchJobs
                .FirstOrDefaultAsync(j => j.JobId == jobId);
                
            if (job != null)
            {
                job.Status = status;
                if (status == BatchJobStatus.Running && !job.StartedAt.HasValue)
                {
                    job.StartedAt = DateTime.UtcNow;
                }
                else if ((status == BatchJobStatus.Completed || status == BatchJobStatus.Failed) && !job.CompletedAt.HasValue)
                {
                    job.CompletedAt = DateTime.UtcNow;
                }
                
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var job = await _context.BatchJobs.FindAsync(id);
            if (job != null)
            {
                _context.BatchJobs.Remove(job);
                await _context.SaveChangesAsync();
            }
        }
    }
}