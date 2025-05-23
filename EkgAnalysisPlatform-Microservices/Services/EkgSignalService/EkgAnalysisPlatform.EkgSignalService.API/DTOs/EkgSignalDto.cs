namespace EkgAnalysisPlatform.EkgSignalService.API.DTOs
{
    public class EkgSignalDto
    {
        public int Id { get; set; }
        public string PatientCode { get; set; } = string.Empty;
        public DateTime RecordedAt { get; set; }
        public double SamplingRate { get; set; }
        public string? Notes { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public string RecordedBy { get; set; } = string.Empty;
        public bool IsProcessed { get; set; }
        public int DataPointsCount { get; set; }
    }
    
    public class CreateEkgSignalDto
    {
        public string PatientCode { get; set; } = string.Empty;
        public DateTime RecordedAt { get; set; }
        public double[] DataPoints { get; set; } = Array.Empty<double>();
        public double SamplingRate { get; set; }
        public string? Notes { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public string RecordedBy { get; set; } = string.Empty;
    }
    
    public class UpdateEkgSignalDto
    {
        public string? Notes { get; set; }
        public bool IsProcessed { get; set; }
    }
}