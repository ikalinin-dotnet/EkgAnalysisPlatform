using EkgAnalysisPlatform.BuildingBlocks.EventBus;
using EkgAnalysisPlatform.BuildingBlocks.EventBus.Events;
using EkgAnalysisPlatform.BatchProcessingService.Domain.Repositories;

namespace EkgAnalysisPlatform.BatchProcessingService.API.EventHandlers
{
    public class AnalysisCompletedEventHandler : IIntegrationEventHandler<AnalysisCompletedEvent>
    {
        private readonly IBatchJobItemRepository _jobItemRepository;
        private readonly ILogger<AnalysisCompletedEventHandler> _logger;

        public AnalysisCompletedEventHandler(
            IBatchJobItemRepository jobItemRepository,
            ILogger<AnalysisCompletedEventHandler> logger)
        {
            _jobItemRepository = jobItemRepository;
            _logger = logger;
        }

        public async Task Handle(AnalysisCompletedEvent @event)
        {
            _logger.LogInformation("Analysis completed for signal {SignalReference} in batch processing", 
                @event.SignalReference);

            // Update batch job item status if this was part of a batch job
            // Implementation depends on your batch job tracking logic

            await Task.CompletedTask;
        }
    }
}