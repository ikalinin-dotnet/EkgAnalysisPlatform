using EkgAnalysisPlatform.AnalysisService.Domain.Models;

namespace EkgAnalysisPlatform.AnalysisService.Domain.Services
{
    public interface IEkgAnalysisEngine
    {
        Task<AnalysisResult> AnalyzeSignalAsync(double[] signalData, string signalReference, string patientCode);
        Task<AnalysisResult> AnalyzeSignalWithAlgorithmAsync(double[] signalData, string signalReference, string patientCode, string algorithmName);
    }
}