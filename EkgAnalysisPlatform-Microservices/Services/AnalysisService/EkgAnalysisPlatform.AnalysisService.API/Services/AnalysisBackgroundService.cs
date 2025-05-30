using EkgAnalysisPlatform.AnalysisService.Domain.Models;
using EkgAnalysisPlatform.AnalysisService.Domain.Repositories;
using EkgAnalysisPlatform.AnalysisService.Domain.Services;
using EkgAnalysisPlatform.BuildingBlocks.EventBus;
using EkgAnalysisPlatform.BuildingBlocks.EventBus.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EkgAnalysisPlatform.AnalysisService.API.Services
{
    public class AnalysisBackgroundService : BackgroundService
    {
        private readonly IAnalysisRequestRepository _requestRepository;
        private readonly IAnalysisResultRepository _resultRepository;
        private readonly IEkgAnalysisEngine _analysisEngine;
        private readonly IEventBus _eventBus;
        private readonly ILogger<AnalysisBackgroundService> _logger;

        public AnalysisBackgroundService(
            IAnalysisRequestRepository requestRepository,
            IAnalysisResultRepository resultRepository,
            IEkgAnalysisEngine analysisEngine,
            IEventBus eventBus,
            ILogger<AnalysisBackgroundService> logger)
        {
            _requestRepository = requestRepository;
            _resultRepository = resultRepository;
            _analysisEngine = analysisEngine;
            _eventBus = eventBus;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Analysis Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingRequests();
                    await Task.Delay(5000, stoppingToken); // Check every 5 seconds
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Analysis Background Service");
                    await Task.Delay(10000, stoppingToken); // Wait longer on error
                }
            }

            _logger.LogInformation("Analysis Background Service stopped");
        }

        private async Task ProcessPendingRequests()
        {
            var pendingRequests = await _requestRepository.GetPendingRequestsAsync();

            foreach (var request in pendingRequests)
            {
                try
                {
                    _logger.LogInformation("Processing analysis request {RequestId} for signal {SignalReference}",
                        request.Id, request.SignalReference);

                    // Update status to in progress
                    await _requestRepository.UpdateStatusAsync(request.Id, AnalysisStatus.InProgress);

                    // In a real implementation, you would fetch the signal data from EkgSignalService
                    // For demo purposes, we'll create mock signal data
                    var mockSignalData = GenerateMockSignalData();

                    // Perform analysis
                    var result = await _analysisEngine.AnalyzeSignalAsync(
                        mockSignalData,
                        request.SignalReference,
                        request.PatientCode);

                    // Save result
                    await _resultRepository.AddAsync(result);

                    // Update request status
                    await _requestRepository.UpdateStatusAsync(request.Id, AnalysisStatus.Completed);

                    // Publish completion event
                    var completionEvent = new AnalysisCompletedEvent
                    {
                        AnalysisResultId = result.Id,
                        PatientCode = result.PatientCode,
                        SignalReference = result.SignalReference,
                        HeartRate = result.HeartRate,
                        HasArrhythmia = result.HasArrhythmia,
                        AnalyzedAt = result.AnalyzedAt
                    };

                    _eventBus.Publish(completionEvent);

                    _logger.LogInformation("Analysis completed for request {RequestId}", request.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing analysis request {RequestId}", request.Id);
                    await _requestRepository.UpdateStatusAsync(request.Id, AnalysisStatus.Failed);
                }
            }
        }

        private double[] GenerateMockSignalData()
        {
            // Generate realistic-looking EKG data for demo
            var data = new double[1000]; // 4 seconds at 250 Hz
            var heartRate = 72.0;
            var sampleRate = 250.0;
            var period = 60.0 / heartRate;
            var samplesPerBeat = period * sampleRate;

            for (int i = 0; i < data.Length; i++)
            {
                var beatPhase = (i % samplesPerBeat) / samplesPerBeat;

                if (beatPhase < 0.1)
                    data[i] = 0.1 * Math.Sin(beatPhase * Math.PI * 2 * 10);
                else if (beatPhase >= 0.2 && beatPhase < 0.25)
                    data[i] = Math.Exp(-Math.Pow((beatPhase - 0.225) * 100, 2)) * 1.0;
                else if (beatPhase >= 0.25 && beatPhase < 0.3)
                    data[i] = Math.Exp(-Math.Pow((beatPhase - 0.275) * 100, 2)) * 2.5;
                else if (beatPhase >= 0.3 && beatPhase < 0.35)
                    data[i] = Math.Exp(-Math.Pow((beatPhase - 0.325) * 100, 2)) * -0.5;
                else if (beatPhase >= 0.4 && beatPhase < 0.7)
                    data[i] = 0.2 * Math.Sin((beatPhase - 0.4) * Math.PI * 2 * 3);
                else
                    data[i] = 0;
            }

            return data;
        }
    }
}