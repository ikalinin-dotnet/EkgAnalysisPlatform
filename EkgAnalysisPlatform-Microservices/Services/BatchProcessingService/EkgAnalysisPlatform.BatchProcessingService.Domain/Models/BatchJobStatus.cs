namespace EkgAnalysisPlatform.BatchProcessingService.Domain.Models
{
    public enum BatchJobStatus
    {
        Pending,
        Running,
        Completed,
        Failed,
        Cancelled
    }
}