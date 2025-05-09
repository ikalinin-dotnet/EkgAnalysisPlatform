namespace EkgAnalysisPlatform.AnalysisService.API.DTOs
{
    public class AnalysisRequestDto
    {
        public int Id { get; set; }
        public string SignalReference { get; set; } = string.Empty;
        public string PatientCode { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string RequestedBy { get; set; } = string.Empty;
        public string AnalysisType { get; set; } = string.Empty;
        public int Priority { get; set; }
    }
    
    public class CreateAnalysisRequestDto
    {
        public string SignalReference { get; set; } = string.Empty;
        public string PatientCode { get; set; } = string.Empty;
        public string RequestedBy { get; set; } = string.Empty;
        public string AnalysisType { get; set; } = "Standard";
        public int Priority { get; set; } = 1;
    }
    
    public class AnalysisResultDto
    {
        public int Id { get; set; }
        public string SignalReference { get; set; } = string.Empty;
        public string PatientCode { get; set; } = string.Empty;
        public double HeartRate { get; set; }
        public bool HasArrhythmia { get; set; }
        public double? QRSDuration { get; set; }
        public double? PRInterval { get; set; }
        public double? QTInterval { get; set; }
        public DateTime AnalyzedAt { get; set; }
        public string AnalyzerVersion { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? DiagnosticNotes { get; set; }
    }
}