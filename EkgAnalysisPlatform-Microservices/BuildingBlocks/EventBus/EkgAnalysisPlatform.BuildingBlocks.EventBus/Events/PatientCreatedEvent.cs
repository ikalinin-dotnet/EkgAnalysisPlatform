namespace EkgAnalysisPlatform.BuildingBlocks.EventBus.Events
{
    public class PatientCreatedEvent : IntegrationEvent
    {
        public int PatientId { get; set; }
        public string PatientCode { get; set; } = string.Empty;
        public string ContactInfo { get; set; } = string.Empty;
    }
}