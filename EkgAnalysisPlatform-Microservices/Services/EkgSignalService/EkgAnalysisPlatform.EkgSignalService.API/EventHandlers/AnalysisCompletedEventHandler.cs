using EkgAnalysisPlatform.BuildingBlocks.EventBus;
using EkgAnalysisPlatform.BuildingBlocks.EventBus.Events;
using EkgAnalysisPlatform.EkgSignalService.Domain.Repositories;

namespace EkgAnalysisPlatform.EkgSignalService.API.EventHandlers
{
    public class AnalysisCompletedEventHandler : IIntegrationEventHandler<AnalysisCompletedEvent>
    {
        private readonly IEkgSignalRepository _signalRepository;
        private readonly ILogger<AnalysisCompletedEventHandler> _logger;

        public AnalysisCompletedEventHandler(
            IEkgSignalRepository signalRepository,
            ILogger<AnalysisCompletedEventHandler> logger)
        {
            _signalRepository = signalRepository;
            _logger = logger;
        }

        public async Task Handle(AnalysisCompletedEvent @event)
        {
            _logger.LogInformation("Analysis completed for signal {SignalReference}", @event.SignalReference);

            // Mark the signal as processed
            if (int.TryParse(@event.SignalReference, out var signalId))
            {
                var signal = await _signalRepository.GetByIdAsync(signalId);
                if (signal != null)
                {
                    signal.IsProcessed = true;
                    await _signalRepository.UpdateAsync(signal);
                    _logger.LogInformation("Signal {SignalId} marked as processed", signalId);
                }
            }
        }
    }
}