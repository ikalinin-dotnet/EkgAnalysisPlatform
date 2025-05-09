using EkgAnalysisPlatform.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EkgAnalysisPlatform.Infrastructure.Data.Contexts
{
    public class AnalysisDbContext : DbContext
    {
        public AnalysisDbContext(DbContextOptions<AnalysisDbContext> options) : base(options)
        {
        }
        
        public DbSet<AnalysisResult> AnalysisResults { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure entity relationships and constraints
            modelBuilder.Entity<AnalysisResult>()
                .HasKey(a => a.Id);
                
            modelBuilder.Entity<AnalysisResult>()
                .Property(a => a.EkgSignalId)
                .IsRequired();
                
            modelBuilder.Entity<AnalysisResult>()
                .Property(a => a.HeartRate)
                .IsRequired();
                
            modelBuilder.Entity<AnalysisResult>()
                .Property(a => a.AnalyzedAt)
                .IsRequired();
                
            // Configure indexes
            modelBuilder.Entity<AnalysisResult>()
                .HasIndex(a => a.EkgSignalId);
                
            modelBuilder.Entity<AnalysisResult>()
                .HasIndex(a => a.AnalyzedAt);
                
            modelBuilder.Entity<AnalysisResult>()
                .HasIndex(a => a.HasArrhythmia);
        }
    }
}