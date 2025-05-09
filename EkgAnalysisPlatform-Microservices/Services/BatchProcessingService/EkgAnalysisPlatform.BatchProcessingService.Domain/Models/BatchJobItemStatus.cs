namespace EkgAnalysisPlatform.BatchProcessingService.Domain.Models
{
    public enum BatchJobItemStatus
    {
        Pending,
        Processing,
        Completed,
        Failed,
        Cancelled
    }
}