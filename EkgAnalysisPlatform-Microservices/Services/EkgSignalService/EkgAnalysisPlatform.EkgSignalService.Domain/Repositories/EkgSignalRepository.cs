using EkgAnalysisPlatform.EkgSignalService.Domain.Models;
using EkgAnalysisPlatform.EkgSignalService.Domain.Repositories;
using EkgAnalysisPlatform.EkgSignalService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EkgAnalysisPlatform.EkgSignalService.Infrastructure.Repositories
{
    public class EkgSignalRepository : IEkgSignalRepository
    {
        private readonly EkgSignalDbContext _context;

        public EkgSignalRepository(EkgSignalDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EkgSignal>> GetAllAsync()
        {
            return await _context.EkgSignals.ToListAsync();
        }

        public async Task<EkgSignal?> GetByIdAsync(int id)
        {
            return await _context.EkgSignals.FindAsync(id);
        }

        public async Task<IEnumerable<EkgSignal>> GetByPatientCodeAsync(string patientCode)
        {
            return await _context.EkgSignals
                .Where(s => s.PatientCode == patientCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<EkgSignal>> GetUnprocessedSignalsAsync()
        {
            return await _context.EkgSignals
                .Where(s => !s.IsProcessed)
                .ToListAsync();
        }

        public async Task<int> AddAsync(EkgSignal signal)
        {
            _context.EkgSignals.Add(signal);
            await _context.SaveChangesAsync();
            return signal.Id;
        }

        public async Task UpdateAsync(EkgSignal signal)
        {
            _context.Entry(signal).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var signal = await _context.EkgSignals.FindAsync(id);
            if (signal != null)
            {
                _context.EkgSignals.Remove(signal);
                await _context.SaveChangesAsync();
            }
        }
    }
}