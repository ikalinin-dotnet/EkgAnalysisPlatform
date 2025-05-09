using EkgAnalysisPlatform.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EkgAnalysisPlatform.Infrastructure.Data.Contexts
{
    public class EkgSignalDbContext : DbContext
    {
        public EkgSignalDbContext(DbContextOptions<EkgSignalDbContext> options) : base(options)
        {
        }
        
        public DbSet<EkgSignal> EkgSignals { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure entity relationships and constraints
            modelBuilder.Entity<EkgSignal>()
                .HasKey(e => e.Id);
                
            modelBuilder.Entity<EkgSignal>()
                .Property(e => e.PatientId)
                .IsRequired();
                
            modelBuilder.Entity<EkgSignal>()
                .Property(e => e.RecordedAt)
                .IsRequired();
                
            modelBuilder.Entity<EkgSignal>()
                .Property(e => e.SamplingRate)
                .IsRequired();
                
            // Configure data points conversion
            modelBuilder.Entity<EkgSignal>()
                .Property(e => e.DataPoints)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(double.Parse).ToArray());
                
            // Configure indexes
            modelBuilder.Entity<EkgSignal>()
                .HasIndex(e => e.PatientId);
                
            modelBuilder.Entity<EkgSignal>()
                .HasIndex(e => e.RecordedAt);
        }
    }
}