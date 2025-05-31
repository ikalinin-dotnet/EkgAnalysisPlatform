using EkgAnalysisPlatform.BuildingBlocks.EventBus;
using EkgAnalysisPlatform.BuildingBlocks.EventBus.Events;

namespace EkgAnalysisPlatform.PatientService.API.EventHandlers
{
    public class AnalysisCompletedEventHandler : IIntegrationEventHandler<AnalysisCompletedEvent>
    {
        private readonly ILogger<AnalysisCompletedEventHandler> _logger;

        public AnalysisCompletedEventHandler(ILogger<AnalysisCompletedEventHandler> logger)
        {
            _logger = logger;
        }

        public async Task Handle(AnalysisCompletedEvent @event)
        {
            _logger.LogInformation("Analysis completed for patient {PatientCode}, Signal {SignalReference}", 
                @event.PatientCode, @event.SignalReference);

            // TODO
            // - Updating patient records
            // - Sending notifications
            // - Triggering other workflows

            await Task.CompletedTask;
        }
    }
}