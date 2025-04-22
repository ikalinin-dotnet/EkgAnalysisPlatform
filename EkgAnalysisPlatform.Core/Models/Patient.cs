namespace EkgAnalysisPlatform.Core.Models;

public class Patient
{
    public int Id { get; set; }
    public string PatientCode { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;
    public ICollection<EkgSignal> EkgSignals { get; set; } = new List<EkgSignal>();
}