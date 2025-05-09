namespace EkgAnalysisPlatform.BatchProcessingService.API.DTOs
{
    public class BatchJobDto
    {
        public int Id { get; set; }
        public string JobId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string JobType { get; set; } = string.Empty;
        public int TotalItems { get; set; }
        public int ProcessedItems { get; set; }
        public int SuccessfulItems { get; set; }
        public int FailedItems { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }
    
    public class CreateBatchJobDto
    {
        public string JobType { get; set; } = string.Empty;
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
        public List<string> ItemIds { get; set; } = new List<string>();
    }
    
    public class BatchJobItemDto
    {
        public int Id { get; set; }
        public string BatchJobId { get; set; } = string.Empty;
        public string ItemId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public int RetryCount { get; set; }
        public DateTime? NextRetryAt { get; set; }
    }
    
    public class ScheduleConfigurationDto
    {
        public int Id { get; set; }
        public string JobType { get; set; } = string.Empty;
        public string CronExpression { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public int MaxBatchSize { get; set; }
        public int MaxConcurrentJobs { get; set; }
        public int MaxRetries { get; set; }
        public int RetryDelayInMinutes { get; set; }
    }
    
    public class CreateScheduleConfigurationDto
    {
        public string JobType { get; set; } = string.Empty;
        public string CronExpression { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
        public int MaxBatchSize { get; set; } = 100;
        public int MaxConcurrentJobs { get; set; } = 1;
        public int MaxRetries { get; set; } = 3;
        public int RetryDelayInMinutes { get; set; } = 5;
    }
}