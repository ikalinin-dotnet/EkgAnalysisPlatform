using EkgAnalysisPlatform.AnalysisService.Domain.Models;
using EkgAnalysisPlatform.AnalysisService.Domain.Services;
using EkgAnalysisPlatform.AnalysisService.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace EkgAnalysisPlatform.AnalysisService.Infrastructure.Services
{
    public class EkgAnalysisEngine : IEkgAnalysisEngine
    {
        private readonly IAnalysisAlgorithmConfigRepository _algorithmRepository;
        private readonly ILogger<EkgAnalysisEngine> _logger;

        public EkgAnalysisEngine(
            IAnalysisAlgorithmConfigRepository algorithmRepository,
            ILogger<EkgAnalysisEngine> logger)
        {
            _algorithmRepository = algorithmRepository;
            _logger = logger;
        }

        public async Task<AnalysisResult> AnalyzeSignalAsync(double[] signalData, string signalReference, string patientCode)
        {
            _logger.LogInformation("Starting EKG analysis for signal {SignalReference}", signalReference);

            try
            {
                // Basic EKG analysis algorithm
                var result = new AnalysisResult
                {
                    SignalReference = signalReference,
                    PatientCode = patientCode,
                    AnalyzedAt = DateTime.UtcNow,
                    AnalyzerVersion = "1.0.0",
                    Status = AnalysisStatus.Completed
                };

                // Calculate heart rate (simplified)
                result.HeartRate = CalculateHeartRate(signalData);

                // Detect arrhythmia (simplified)
                result.HasArrhythmia = DetectArrhythmia(signalData);

                // Calculate intervals (simplified)
                result.QRSDuration = CalculateQRSDuration(signalData);
                result.PRInterval = CalculatePRInterval(signalData);
                result.QTInterval = CalculateQTInterval(signalData);

                // Generate diagnostic notes
                result.DiagnosticNotes = GenerateDiagnosticNotes(result);

                _logger.LogInformation("EKG analysis completed for signal {SignalReference} with heart rate {HeartRate} BPM", 
                    signalReference, result.HeartRate);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing EKG signal {SignalReference}", signalReference);
                throw;
            }
        }

        public async Task<AnalysisResult> AnalyzeSignalWithAlgorithmAsync(double[] signalData, string signalReference, string patientCode, string algorithmName)
        {
            var algorithm = await _algorithmRepository.GetActiveAlgorithmAsync(algorithmName);
            if (algorithm == null)
            {
                _logger.LogWarning("Algorithm {AlgorithmName} not found, using default analysis", algorithmName);
                return await AnalyzeSignalAsync(signalData, signalReference, patientCode);
            }

            _logger.LogInformation("Using algorithm {AlgorithmName} version {Version} for analysis", 
                algorithm.AlgorithmName, algorithm.Version);

            // Use the specific algorithm (implementation would depend on algorithm parameters)
            var result = await AnalyzeSignalAsync(signalData, signalReference, patientCode);
            result.AnalyzerVersion = $"{algorithm.AlgorithmName} {algorithm.Version}";
            
            return result;
        }

        private double CalculateHeartRate(double[] signalData)
        {
            // Simplified heart rate calculation
            // In real implementation, you would use R-peak detection algorithms
            var sampleRate = 250.0; // Hz
            var duration = signalData.Length / sampleRate; // seconds
            
            // Simplified: assume average heart rate between 60-100 BPM
            // This would be replaced with actual R-peak detection
            var random = new Random();
            return 60 + random.NextDouble() * 40; // Random between 60-100 for demo
        }

        private bool DetectArrhythmia(double[] signalData)
        {
            // Simplified arrhythmia detection
            // Real implementation would analyze rhythm irregularities
            var heartRate = CalculateHeartRate(signalData);
            return heartRate < 60 || heartRate > 100; // Simplified criteria
        }

        private double CalculateQRSDuration(double[] signalData)
        {
            // Simplified QRS duration calculation
            // Normal QRS duration is 0.06-0.10 seconds
            var random = new Random();
            return 0.06 + random.NextDouble() * 0.04;
        }

        private double CalculatePRInterval(double[] signalData)
        {
            // Simplified PR interval calculation
            // Normal PR interval is 0.12-0.20 seconds
            var random = new Random();
            return 0.12 + random.NextDouble() * 0.08;
        }

        private double CalculateQTInterval(double[] signalData)
        {
            // Simplified QT interval calculation
            // Normal QT interval is 0.36-0.44 seconds
            var random = new Random();
            return 0.36 + random.NextDouble() * 0.08;
        }

        private string GenerateDiagnosticNotes(AnalysisResult result)
        {
            var notes = new List<string>();

            if (result.HeartRate < 60)
                notes.Add("Bradycardia detected");
            else if (result.HeartRate > 100)
                notes.Add("Tachycardia detected");
            else
                notes.Add("Normal sinus rhythm");

            if (result.HasArrhythmia)
                notes.Add("Irregular rhythm patterns detected");

            if (result.QRSDuration > 0.12)
                notes.Add("Prolonged QRS duration");

            if (result.PRInterval > 0.20)
                notes.Add("First-degree AV block");

            if (result.QTInterval > 0.44)
                notes.Add("Prolonged QT interval");

            return string.Join("; ", notes);
        }
    }
}