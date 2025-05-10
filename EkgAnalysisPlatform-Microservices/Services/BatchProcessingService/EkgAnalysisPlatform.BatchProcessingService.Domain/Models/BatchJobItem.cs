namespace EkgAnalysisPlatform.BatchProcessingService.Domain.Models
{
    public class BatchJobItem
    {
        public int Id { get; set; }
        public string BatchJobId { get; set; } = string.Empty;
        public string ItemId { get; set; } = string.Empty; // Reference to the item being processed (SignalId, etc.)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
        public BatchJobItemStatus Status { get; set; } = BatchJobItemStatus.Pending;
        public string ErrorMessage { get; set; } = string.Empty;
        public int RetryCount { get; set; } = 0;
        public DateTime? NextRetryAt { get; set; }
    }
}