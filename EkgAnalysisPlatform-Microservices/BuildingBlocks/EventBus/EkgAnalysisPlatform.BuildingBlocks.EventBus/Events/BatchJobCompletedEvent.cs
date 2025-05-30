namespace EkgAnalysisPlatform.BuildingBlocks.EventBus.Events
{
    public class BatchJobCompletedEvent : IntegrationEvent
    {
        public string JobId { get; set; } = string.Empty;
        public string JobType { get; set; } = string.Empty;
        public int TotalItems { get; set; }
        public int SuccessfulItems { get; set; }
        public int FailedItems { get; set; }
        public DateTime CompletedAt { get; set; }
    }
}