namespace EkgAnalysisPlatform.BatchProcessingService.Domain.Models
{
    public class ScheduleConfiguration
    {
        public int Id { get; set; }
        public string JobType { get; set; } = string.Empty;
        public string CronExpression { get; set; } = string.Empty; // CRON format for scheduling
        public bool IsEnabled { get; set; } = true;
        public int MaxBatchSize { get; set; } = 100;
        public int MaxConcurrentJobs { get; set; } = 1;
        public int MaxRetries { get; set; } = 3;
        public int RetryDelayInMinutes { get; set; } = 5;
    }
}