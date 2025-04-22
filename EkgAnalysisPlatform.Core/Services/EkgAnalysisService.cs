using EkgAnalysisPlatform.Core.Interfaces;
using EkgAnalysisPlatform.Core.Models;

namespace EkgAnalysisPlatform.Core.Services;

public class EkgAnalysisService : IEkgAnalysisService
{
    public async Task<AnalysisResult> AnalyzeSignalAsync(EkgSignal signal)
    {
        var dataPoints = signal.DataPoints;
        var samplingRate = signal.SamplingRate;
        
        var heartRate = await CalculateHeartRateAsync(dataPoints, samplingRate);
        var hasArrhythmia = await DetectArrhythmiaAsync(dataPoints, samplingRate);
        
        return new AnalysisResult
        {
            EkgSignalId = signal.Id,
            HeartRate = heartRate,
            HasArrhythmia = hasArrhythmia,
            AnalyzedAt = DateTime.UtcNow
        };
    }
    
    public async Task<double> CalculateHeartRateAsync(double[] dataPoints, double samplingRate)
    {
        // Simple R-peak detection for demo purposes
        var rPeaks = await DetectRPeaksAsync(dataPoints);
        
        // Calculate average RR interval
        double avgRRinterval = 0;
        if (rPeaks.Length > 1)
        {
            double sum = 0;
            for (int i = 1; i < rPeaks.Length; i++)
            {
                sum += rPeaks[i] - rPeaks[i - 1];
            }
            avgRRinterval = sum / (rPeaks.Length - 1);
        }
        
        // Convert to seconds and calculate HR
        double avgRRintervalSeconds = avgRRinterval / samplingRate;
        double heartRate = 60 / avgRRintervalSeconds;
        
        return heartRate;
    }
    
    public async Task<bool> DetectArrhythmiaAsync(double[] dataPoints, double samplingRate)
    {
        // Simple arrhythmia detection for demo
        var rPeaks = await DetectRPeaksAsync(dataPoints);
        
        // Calculate RR intervals
        var rrIntervals = new List<double>();
        for (int i = 1; i < rPeaks.Length; i++)
        {
            rrIntervals.Add(rPeaks[i] - rPeaks[i - 1]);
        }
        
        // Check for irregular RR intervals (simplified)
        if (rrIntervals.Count < 2) return false;
        
        double mean = rrIntervals.Average();
        double variance = rrIntervals.Sum(rr => Math.Pow(rr - mean, 2)) / rrIntervals.Count;
        double stdDev = Math.Sqrt(variance);
        
        // If standard deviation is high relative to mean, indicates irregular rhythm
        return (stdDev / mean) > 0.1;
    }
    
    public async Task<double[]> DetectRPeaksAsync(double[] dataPoints)
    {
        // Simple threshold-based R peak detection for demo
        var peakIndices = new List<double>();
        double threshold = 0.7 * dataPoints.Max();
        
        for (int i = 1; i < dataPoints.Length - 1; i++)
        {
            if (dataPoints[i] > threshold && 
                dataPoints[i] > dataPoints[i-1] && 
                dataPoints[i] > dataPoints[i+1])
            {
                peakIndices.Add(i);
            }
        }
        
        return peakIndices.ToArray();
    }
}