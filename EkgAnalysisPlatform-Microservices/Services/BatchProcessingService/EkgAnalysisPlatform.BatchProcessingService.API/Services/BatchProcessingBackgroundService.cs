using EkgAnalysisPlatform.BatchProcessingService.Domain.Models;
using EkgAnalysisPlatform.BatchProcessingService.Domain.Repositories;
using EkgAnalysisPlatform.BuildingBlocks.EventBus;
using EkgAnalysisPlatform.BuildingBlocks.EventBus.Events;

namespace EkgAnalysisPlatform.BatchProcessingService.API.Services
{
    public class BatchProcessingBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BatchProcessingBackgroundService> _logger;

        public BatchProcessingBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<BatchProcessingBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Batch Processing Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var jobRepository = scope.ServiceProvider.GetRequiredService<IBatchJobRepository>();
                    var jobItemRepository = scope.ServiceProvider.GetRequiredService<IBatchJobItemRepository>();
                    var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

                    await ProcessPendingJobs(jobRepository, jobItemRepository, eventBus);
                    await ProcessRetryItems(jobItemRepository);
                    
                    await Task.Delay(5000, stoppingToken); // Check every 5 seconds
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Batch Processing Background Service");
                    await Task.Delay(10000, stoppingToken); // Wait longer on error
                }
            }

            _logger.LogInformation("Batch Processing Background Service stopped");
        }

        private async Task ProcessPendingJobs(
            IBatchJobRepository jobRepository, 
            IBatchJobItemRepository jobItemRepository,
            IEventBus eventBus)
        {
            var pendingJobs = await jobRepository.GetByStatusAsync(BatchJobStatus.Pending);

            foreach (var job in pendingJobs)
            {
                try
                {
                    _logger.LogInformation("Starting batch job {JobId}", job.JobId);

                    // Update job status to running
                    await jobRepository.UpdateStatusAsync(job.JobId, BatchJobStatus.Running);

                    // Get all items for this job
                    var jobItems = await jobItemRepository.GetAllByBatchJobIdAsync(job.JobId);

                    // Process items based on job type
                    switch (job.JobType)
                    {
                        case "SignalAnalysis":
                            await ProcessSignalAnalysisJob(job, jobItems, jobItemRepository, eventBus);
                            break;
                        default:
                            _logger.LogWarning("Unknown job type: {JobType}", job.JobType);
                            break;
                    }

                    // Update job completion status
                    var completedItems = await jobItemRepository.GetCountByStatusAsync(job.JobId, BatchJobItemStatus.Completed);
                    var failedItems = await jobItemRepository.GetCountByStatusAsync(job.JobId, BatchJobItemStatus.Failed);
                    
                    job.SuccessfulItems = completedItems;
                    job.FailedItems = failedItems;
                    job.ProcessedItems = completedItems + failedItems;

                    await jobRepository.UpdateAsync(job);

                    if (job.ProcessedItems >= job.TotalItems)
                    {
                        await jobRepository.UpdateStatusAsync(job.JobId, BatchJobStatus.Completed);
                        
                        // Publish completion event
                        var completionEvent = new BatchJobCompletedEvent
                        {
                            JobId = job.JobId,
                            JobType = job.JobType,
                            TotalItems = job.TotalItems,
                            SuccessfulItems = job.SuccessfulItems,
                            FailedItems = job.FailedItems,
                            CompletedAt = DateTime.UtcNow
                        };
                        
                        eventBus.Publish(completionEvent);
                        
                        _logger.LogInformation("Batch job {JobId} completed. Success: {Success}, Failed: {Failed}", 
                            job.JobId, job.SuccessfulItems, job.FailedItems);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing batch job {JobId}", job.JobId);
                    await jobRepository.UpdateStatusAsync(job.JobId, BatchJobStatus.Failed);
                }
            }
        }

        private async Task ProcessSignalAnalysisJob(
            BatchJob job,
            IEnumerable<BatchJobItem> jobItems,
            IBatchJobItemRepository jobItemRepository,
            IEventBus eventBus)
        {
            // For each signal, create an analysis request
            foreach (var item in jobItems.Where(i => i.Status == BatchJobItemStatus.Pending))
            {
                try
                {
                    await jobItemRepository.UpdateStatusAsync(item.Id, BatchJobItemStatus.Processing);

                    // In a real implementation, you would:
                    // 1. Fetch the signal data from EkgSignalService
                    // 2. Submit analysis request to AnalysisService
                    // 3. Wait for completion or handle asynchronously

                    // For demo purposes, we'll simulate processing
                    await Task.Delay(1000); // Simulate processing time

                    // Mark as completed
                    await jobItemRepository.UpdateStatusAsync(item.Id, BatchJobItemStatus.Completed);
                    
                    _logger.LogInformation("Processed batch item {ItemId} for job {JobId}", 
                        item.ItemId, job.JobId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing batch item {ItemId}", item.ItemId);
                    
                    var updatedItem = await jobItemRepository.GetByIdAsync(item.Id);
                    if (updatedItem != null)
                    {
                        updatedItem.Status = BatchJobItemStatus.Failed;
                        updatedItem.ErrorMessage = ex.Message;
                        updatedItem.RetryCount++;
                        
                        // Schedule retry if under retry limit
                        if (updatedItem.RetryCount < 3)
                        {
                            updatedItem.NextRetryAt = DateTime.UtcNow.AddMinutes(5 * updatedItem.RetryCount);
                        }
                        
                        await jobItemRepository.UpdateAsync(updatedItem);
                    }
                }
            }
        }

        private async Task ProcessRetryItems(IBatchJobItemRepository jobItemRepository)
        {
            var retryItems = await jobItemRepository.GetItemsForRetryAsync();

            foreach (var item in retryItems)
            {
                try
                {
                    _logger.LogInformation("Retrying batch item {ItemId}, attempt {RetryCount}", 
                        item.ItemId, item.RetryCount + 1);

                    // Reset for retry
                    item.Status = BatchJobItemStatus.Pending;
                    item.NextRetryAt = null;
                    await jobItemRepository.UpdateAsync(item);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error setting up retry for item {ItemId}", item.ItemId);
                }
            }
        }
    }
}