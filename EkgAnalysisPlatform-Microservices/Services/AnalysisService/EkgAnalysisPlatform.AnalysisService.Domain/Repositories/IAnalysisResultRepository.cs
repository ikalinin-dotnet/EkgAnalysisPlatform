namespace EkgAnalysisPlatform.AnalysisService.Domain.Repositories
{
    public interface IAnalysisResultRepository
    {
        Task<IEnumerable<AnalysisResult>> GetAllAsync();
        Task<AnalysisResult?> GetByIdAsync(int id);
        Task<IEnumerable<AnalysisResult>> GetByPatientCodeAsync(string patientCode);
        Task<AnalysisResult?> GetBySignalReferenceAsync(string signalReference);
        Task<int> AddAsync(AnalysisResult result);
        Task UpdateAsync(AnalysisResult result);
        Task DeleteAsync(int id);
    }
}