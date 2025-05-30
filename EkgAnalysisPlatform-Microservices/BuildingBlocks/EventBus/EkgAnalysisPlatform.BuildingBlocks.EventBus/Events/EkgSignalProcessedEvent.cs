namespace EkgAnalysisPlatform.BuildingBlocks.EventBus.Events
{
    public class EkgSignalProcessedEvent : IntegrationEvent
    {
        public int SignalId { get; set; }
        public string PatientCode { get; set; } = string.Empty;
        public string SignalReference { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
    }
}