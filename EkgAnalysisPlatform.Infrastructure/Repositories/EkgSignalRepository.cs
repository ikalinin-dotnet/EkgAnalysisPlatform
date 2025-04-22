using EkgAnalysisPlatform.Core.Interfaces;
using EkgAnalysisPlatform.Core.Models;
using EkgAnalysisPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EkgAnalysisPlatform.Infrastructure.Repositories;

public class EkgSignalRepository : IEkgSignalRepository
{
    private readonly ApplicationDbContext _context;

    public EkgSignalRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EkgSignal>> GetAllAsync()
    {
        return await _context.EkgSignals.ToListAsync();
    }

    public async Task<EkgSignal?> GetByIdAsync(int id)
    {
        return await _context.EkgSignals
            .Include(e => e.Patient)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<EkgSignal>> GetByPatientIdAsync(int patientId)
    {
        return await _context.EkgSignals
            .Where(e => e.PatientId == patientId)
            .ToListAsync();
    }

    public async Task<IEnumerable<EkgSignal>> GetUnanalyzedSignalsAsync()
    {
        return await _context.EkgSignals
            .Where(s => !_context.AnalysisResults.Any(a => a.EkgSignalId == s.Id))
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
        _context.EkgSignals.Update(signal);
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