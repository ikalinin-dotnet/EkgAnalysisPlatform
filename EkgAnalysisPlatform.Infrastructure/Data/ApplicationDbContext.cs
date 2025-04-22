using EkgAnalysisPlatform.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EkgAnalysisPlatform.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    public DbSet<Patient> Patients { get; set; }
    public DbSet<EkgSignal> EkgSignals { get; set; }
    public DbSet<AnalysisResult> AnalysisResults { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure EkgSignal entity
        modelBuilder.Entity<EkgSignal>()
            .Property(e => e.DataPoints)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(double.Parse).ToArray());
                    
        // Configure relationships
        modelBuilder.Entity<Patient>()
            .HasMany(p => p.EkgSignals)
            .WithOne(e => e.Patient)
            .HasForeignKey(e => e.PatientId);
            
        modelBuilder.Entity<EkgSignal>()
            .HasOne(e => e.Patient)
            .WithMany(p => p.EkgSignals)
            .HasForeignKey(e => e.PatientId);
    }
}