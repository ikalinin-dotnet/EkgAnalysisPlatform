namespace EkgAnalysisPlatform.Core.Models;

public class AnalysisResult
{
    public int Id { get; set; }
    public int EkgSignalId { get; set; }
    public double HeartRate { get; set; }
    public bool HasArrhythmia { get; set; }
    public double? QRSDuration { get; set; }
    public double? PRInterval { get; set; }
    public double? QTInterval { get; set; }
    public DateTime AnalyzedAt { get; set; }
    public EkgSignal? EkgSignal { get; set; }
}