using EkgAnalysisPlatform.BatchProcessingService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EkgAnalysisPlatform.BatchProcessingService.Infrastructure.Data
{
    public class BatchDbContext : DbContext
    {
        public BatchDbContext(DbContextOptions<BatchDbContext> options) : base(options)
        {
        }
        
        public DbSet<BatchJob> BatchJobs { get; set; }
        public DbSet<BatchJobItem> BatchJobItems { get; set; }
        public DbSet<ScheduleConfiguration> ScheduleConfigurations { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure BatchJob entity
            modelBuilder.Entity<BatchJob>()
                .HasKey(j => j.Id);
                
            modelBuilder.Entity<BatchJob>()
                .Property(j => j.JobId)
                .IsRequired();
                
            modelBuilder.Entity<BatchJob>()
                .HasIndex(j => j.JobId)
                .IsUnique();
                
            modelBuilder.Entity<BatchJob>()
                .Property(j => j.JobType)
                .IsRequired();
                
            // Configure BatchJobItem entity
            modelBuilder.Entity<BatchJobItem>()
                .HasKey(i => i.Id);
                
            modelBuilder.Entity<BatchJobItem>()
                .Property(i => i.BatchJobId)
                .IsRequired();
                
            modelBuilder.Entity<BatchJobItem>()
                .Property(i => i.ItemId)
                .IsRequired();
                
            modelBuilder.Entity<BatchJobItem>()
                .HasIndex(i => new { i.BatchJobId, i.ItemId })
                .IsUnique();
                
            // Configure ScheduleConfiguration entity
            modelBuilder.Entity<ScheduleConfiguration>()
                .HasKey(s => s.Id);
                
            modelBuilder.Entity<ScheduleConfiguration>()
                .Property(s => s.JobType)
                .IsRequired();
                
            modelBuilder.Entity<ScheduleConfiguration>()
                .HasIndex(s => s.JobType)
                .IsUnique();
                
            modelBuilder.Entity<ScheduleConfiguration>()
                .Property(s => s.CronExpression)
                .IsRequired();
                
            // Store Dictionary as JSON
            modelBuilder.Entity<BatchJob>()
                .Property(j => j.Parameters)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, string>()
                );
        }
    }
}