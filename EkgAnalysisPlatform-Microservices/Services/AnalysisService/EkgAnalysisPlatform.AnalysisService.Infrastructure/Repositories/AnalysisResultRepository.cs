using EkgAnalysisPlatform.AnalysisService.Domain.Models;
using EkgAnalysisPlatform.AnalysisService.Domain.Repositories;
using EkgAnalysisPlatform.AnalysisService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EkgAnalysisPlatform.AnalysisService.Infrastructure.Repositories
{
    public class AnalysisResultRepository : IAnalysisResultRepository
    {
        private readonly AnalysisDbContext _context;

        public AnalysisResultRepository(AnalysisDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AnalysisResult>> GetAllAsync()
        {
            return await _context.AnalysisResults.ToListAsync();
        }

        public async Task<AnalysisResult?> GetByIdAsync(int id)
        {
            return await _context.AnalysisResults.FindAsync(id);
        }

        public async Task<IEnumerable<AnalysisResult>> GetByPatientCodeAsync(string patientCode)
        {
            return await _context.AnalysisResults
                .Where(r => r.PatientCode == patientCode)
                .ToListAsync();
        }

        public async Task<AnalysisResult?> GetBySignalReferenceAsync(string signalReference)
        {
            return await _context.AnalysisResults
                .FirstOrDefaultAsync(r => r.SignalReference == signalReference);
        }

        public async Task<int> AddAsync(AnalysisResult result)
        {
            _context.AnalysisResults.Add(result);
            await _context.SaveChangesAsync();
            return result.Id;
        }

        public async Task UpdateAsync(AnalysisResult result)
        {
            _context.Entry(result).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var result = await _context.AnalysisResults.FindAsync(id);
            if (result != null)
            {
                _context.AnalysisResults.Remove(result);
                await _context.SaveChangesAsync();
            }
        }
    }
}