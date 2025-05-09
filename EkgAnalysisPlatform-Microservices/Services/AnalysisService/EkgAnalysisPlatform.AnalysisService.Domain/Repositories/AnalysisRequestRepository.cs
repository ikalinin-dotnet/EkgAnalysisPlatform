using EkgAnalysisPlatform.AnalysisService.Domain.Models;
using EkgAnalysisPlatform.AnalysisService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EkgAnalysisPlatform.AnalysisService.Infrastructure.Repositories
{
    public class AnalysisRequestRepository : IAnalysisRequestRepository
    {
        private readonly AnalysisDbContext _context;

        public AnalysisRequestRepository(AnalysisDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AnalysisRequest>> GetAllAsync()
        {
            return await _context.AnalysisRequests.ToListAsync();
        }

        public async Task<AnalysisRequest?> GetByIdAsync(int id)
        {
            return await _context.AnalysisRequests.FindAsync(id);
        }

        public async Task<IEnumerable<AnalysisRequest>> GetByStatusAsync(AnalysisStatus status)
        {
            return await _context.AnalysisRequests
                .Where(r => r.Status == status)
                .ToListAsync();
        }

        public async Task<IEnumerable<AnalysisRequest>> GetPendingRequestsAsync()
        {
            return await _context.AnalysisRequests
                .Where(r => r.Status == AnalysisStatus.Pending)
                .OrderBy(r => r.RequestedAt)
                .ToListAsync();
        }

        public async Task<int> AddAsync(AnalysisRequest request)
        {
            _context.AnalysisRequests.Add(request);
            await _context.SaveChangesAsync();
            return request.Id;
        }

        public async Task UpdateAsync(AnalysisRequest request)
        {
            _context.Entry(request).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(int id, AnalysisStatus status)
        {
            var request = await _context.AnalysisRequests.FindAsync(id);
            if (request != null)
            {
                request.Status = status;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var request = await _context.AnalysisRequests.FindAsync(id);
            if (request != null)
            {
                _context.AnalysisRequests.Remove(request);
                await _context.SaveChangesAsync();
            }
        }
    }
}