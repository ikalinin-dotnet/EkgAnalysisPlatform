using EkgAnalysisPlatform.AnalysisService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EkgAnalysisPlatform.AnalysisService.Infrastructure.Data
{
    public class AnalysisDbContext : DbContext
    {
        public AnalysisDbContext(DbContextOptions<AnalysisDbContext> options) : base(options)
        {
        }
        
        public DbSet<AnalysisRequest> AnalysisRequests { get; set; }
        public DbSet<AnalysisResult> AnalysisResults { get; set; }
        public DbSet<AnalysisAlgorithmConfig> AlgorithmConfigs { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure entity relationships and constraints
            modelBuilder.Entity<AnalysisResult>()
                .HasKey(a => a.Id);
                
            modelBuilder.Entity<AnalysisResult>()
                .Property(a => a.SignalReference)
                .IsRequired();
                
            modelBuilder.Entity<AnalysisResult>()
                .Property(a => a.HeartRate)
                .IsRequired();
                
            modelBuilder.Entity<AnalysisResult>()
                .Property(a => a.AnalyzedAt)
                .IsRequired();
                
            // Configure indexes
            modelBuilder.Entity<AnalysisResult>()
                .HasIndex(a => a.SignalReference);
                
            modelBuilder.Entity<AnalysisResult>()
                .HasIndex(a => a.AnalyzedAt);
                
            modelBuilder.Entity<AnalysisResult>()
                .HasIndex(a => a.HasArrhythmia);

            // Configure AnalysisRequest entity
            modelBuilder.Entity<AnalysisRequest>()
                .HasKey(a => a.Id);

            modelBuilder.Entity<AnalysisRequest>()
                .Property(a => a.SignalReference)
                .IsRequired();

            modelBuilder.Entity<AnalysisRequest>()
                .Property(a => a.PatientCode)
                .IsRequired();

            // Configure AlgorithmConfig entity
            modelBuilder.Entity<AnalysisAlgorithmConfig>()
                .HasKey(a => a.Id);

            modelBuilder.Entity<AnalysisAlgorithmConfig>()
                .Property(a => a.AlgorithmName)
                .IsRequired();

            modelBuilder.Entity<AnalysisAlgorithmConfig>()
                .Property(a => a.Version)
                .IsRequired();
                
            // Store Dictionary as JSON
            modelBuilder.Entity<AnalysisAlgorithmConfig>()
                .Property(a => a.Parameters)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, new System.Text.Json.JsonSerializerOptions()),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, new System.Text.Json.JsonSerializerOptions()) ?? new Dictionary<string, string>()
                );
        }
    }
}