using System;

namespace EkgAnalysisPlatform.AnalysisService.Domain.Models
{
    public class AnalysisResult
    {
        public int Id { get; set; }
        public string SignalReference { get; set; } = string.Empty; // Reference to EkgSignal in the Signal Service
        public string PatientCode { get; set; } = string.Empty; // For direct patient reference
        public double HeartRate { get; set; }
        public bool HasArrhythmia { get; set; }
        public double? QRSDuration { get; set; }
        public double? PRInterval { get; set; }
        public double? QTInterval { get; set; }
        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
        public string AnalyzerVersion { get; set; } = string.Empty; // Track analyzer algorithm version
        public AnalysisStatus Status { get; set; } = AnalysisStatus.Completed;
        public string? DiagnosticNotes { get; set; }
    }
}