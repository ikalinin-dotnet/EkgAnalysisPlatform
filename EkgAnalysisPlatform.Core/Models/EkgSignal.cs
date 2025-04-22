namespace EkgAnalysisPlatform.Core.Models;

public class EkgSignal
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public DateTime RecordedAt { get; set; }
    public double[] DataPoints { get; set; } = Array.Empty<double>();
    public double SamplingRate { get; set; } // Hz
    public string? Notes { get; set; }
    public Patient? Patient { get; set; }
}