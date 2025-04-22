using EkgAnalysisPlatform.Core.Models;

namespace EkgAnalysisPlatform.Core.Interfaces;

public interface IEkgSignalRepository
{
    Task<IEnumerable<EkgSignal>> GetAllAsync();
    Task<EkgSignal?> GetByIdAsync(int id);
    Task<IEnumerable<EkgSignal>> GetByPatientIdAsync(int patientId);
    Task<int> AddAsync(EkgSignal signal);
    Task UpdateAsync(EkgSignal signal);
    Task DeleteAsync(int id);
}