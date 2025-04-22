using EkgAnalysisPlatform.Core.Models;

namespace EkgAnalysisPlatform.Core.Interfaces;

public interface IEkgAnalysisService
{
    Task<AnalysisResult> AnalyzeSignalAsync(EkgSignal signal);
    Task<double> CalculateHeartRateAsync(double[] dataPoints, double samplingRate);
    Task<bool> DetectArrhythmiaAsync(double[] dataPoints, double samplingRate);
    Task<double[]> DetectRPeaksAsync(double[] dataPoints);
}