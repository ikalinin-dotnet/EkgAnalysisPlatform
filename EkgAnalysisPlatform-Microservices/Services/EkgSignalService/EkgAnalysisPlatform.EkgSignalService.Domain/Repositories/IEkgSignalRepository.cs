namespace EkgAnalysisPlatform.EkgSignalService.Domain.Repositories
{
    public interface IEkgSignalRepository
    {
        Task<IEnumerable<EkgSignal>> GetAllAsync();
        Task<EkgSignal?> GetByIdAsync(int id);
        Task<IEnumerable<EkgSignal>> GetByPatientCodeAsync(string patientCode);
        Task<IEnumerable<EkgSignal>> GetUnprocessedSignalsAsync();
        Task<int> AddAsync(EkgSignal signal);
        Task UpdateAsync(EkgSignal signal);
        Task DeleteAsync(int id);
    }
}