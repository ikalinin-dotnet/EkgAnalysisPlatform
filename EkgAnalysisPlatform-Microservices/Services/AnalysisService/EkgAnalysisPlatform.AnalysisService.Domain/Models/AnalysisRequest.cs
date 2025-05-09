using System;

namespace EkgAnalysisPlatform.AnalysisService.Domain.Models
{
    public class AnalysisRequest
    {
        public int Id { get; set; }
        public string SignalReference { get; set; } = string.Empty;
        public string PatientCode { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public AnalysisStatus Status { get; set; } = AnalysisStatus.Pending;
        public string RequestedBy { get; set; } = string.Empty;
        public string AnalysisType { get; set; } = "Standard"; // Could be "Standard", "Extended", etc.
        public int Priority { get; set; } = 1; // 1-5 scale where 5 is highest priority
    }
}