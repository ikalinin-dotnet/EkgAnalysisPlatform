using System;
using System.Collections.Generic;

namespace EkgAnalysisPlatform.BatchProcessingService.Domain.Models
{
    public class BatchJob
    {
        public int Id { get; set; }
        public string JobId { get; set; } = Guid.NewGuid().ToString();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public BatchJobStatus Status { get; set; } = BatchJobStatus.Pending;
        public string JobType { get; set; } = string.Empty;
        public int TotalItems { get; set; }
        public int ProcessedItems { get; set; }
        public int SuccessfulItems { get; set; }
        public int FailedItems { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }
}