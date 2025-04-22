using EkgAnalysisPlatform.Core.Models;
using EkgAnalysisPlatform.Core.Services;
using Xunit;

namespace EkgAnalysisPlatform.Tests;

public class EkgAnalysisServiceTests
{
    private readonly EkgAnalysisService _service;
    
    public EkgAnalysisServiceTests()
    {
        _service = new EkgAnalysisService();
    }
    
    [Fact]
    public async Task CalculateHeartRate_WithNormalEkgData_ReturnsExpectedHeartRate()
    {
        // Arrange
        // Create a synthetic EKG signal with R peaks at regular intervals
        // For 60 BPM, with 250Hz sampling rate, peaks should be 250 samples apart
        var dataPoints = new double[2500]; // 10 seconds of data
        for (int i = 0; i < dataPoints.Length; i++)
        {
            dataPoints[i] = 0.1; // baseline
            
            // Add R peaks every 250 samples (60 BPM)
            if (i % 250 == 0)
            {
                dataPoints[i] = 1.0; // R peak
            }
        }
        
        double samplingRate = 250; // Hz
        
        // Act
        var heartRate = await _service.CalculateHeartRateAsync(dataPoints, samplingRate);
        
        // Assert
        Assert.InRange(heartRate, 55, 65); // Allow for some calculation variance
    }
    
    [Fact]
    public async Task DetectArrhythmia_WithRegularRhythm_ReturnsFalse()
    {
        // Arrange
        // Create a synthetic EKG signal with regular R peaks
        var dataPoints = new double[2500];
        for (int i = 0; i < dataPoints.Length; i++)
        {
            dataPoints[i] = 0.1;
            if (i % 250 == 0)
            {
                dataPoints[i] = 1.0;
            }
        }
        
        double samplingRate = 250;
        
        // Act
        var hasArrhythmia = await _service.DetectArrhythmiaAsync(dataPoints, samplingRate);
        
        // Assert
        Assert.False(hasArrhythmia);
    }
    
    [Fact]
    public async Task DetectArrhythmia_WithIrregularRhythm_ReturnsTrue()
    {
        // Arrange
        // Create a synthetic EKG with irregular R peaks
        var dataPoints = new double[2500];
        
        // Add R peaks at irregular intervals
        int[] peakPositions = { 250, 450, 800, 950, 1300, 1700, 1850, 2200 };
        
        for (int i = 0; i < dataPoints.Length; i++)
        {
            dataPoints[i] = 0.1;
            if (peakPositions.Contains(i))
            {
                dataPoints[i] = 1.0;
            }
        }
        
        double samplingRate = 250;
        
        // Act
        var hasArrhythmia = await _service.DetectArrhythmiaAsync(dataPoints, samplingRate);
        
        // Assert
        Assert.True(hasArrhythmia);
    }
}