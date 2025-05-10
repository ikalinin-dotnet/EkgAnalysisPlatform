namespace EkgAnalysisPlatform.EkgSignalService.Domain.Models
{
    public class EkgSignal
    {
        public int Id { get; set; }
        public string PatientCode { get; set; } = string.Empty;
        public DateTime RecordedAt { get; set; }
        public double[] DataPoints { get; set; } = Array.Empty<double>();
        public double SamplingRate { get; set; } // Hz
        public string? Notes { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public string RecordedBy { get; set; } = string.Empty;
        public bool IsProcessed { get; set; } = false;
    }
}