namespace EkgAnalysisPlatform.BuildingBlocks.EventBus.Events
{
    public class AnalysisCompletedEvent : IntegrationEvent
    {
        public int AnalysisResultId { get; set; }
        public string PatientCode { get; set; } = string.Empty;
        public string SignalReference { get; set; } = string.Empty;
        public double HeartRate { get; set; }
        public bool HasArrhythmia { get; set; }
        public DateTime AnalyzedAt { get; set; }
    }
}