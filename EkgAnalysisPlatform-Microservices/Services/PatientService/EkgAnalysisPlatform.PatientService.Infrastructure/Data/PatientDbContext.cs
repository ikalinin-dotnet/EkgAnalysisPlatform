using EkgAnalysisPlatform.PatientService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EkgAnalysisPlatform.PatientService.Infrastructure.Data
{
    public class PatientDbContext : DbContext
    {
        public PatientDbContext(DbContextOptions<PatientDbContext> options) : base(options)
        {
        }
        
        public DbSet<Patient> Patients { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure entity relationships and constraints
            modelBuilder.Entity<Patient>()
                .HasKey(p => p.Id);
                
            modelBuilder.Entity<Patient>()
                .Property(p => p.PatientCode)
                .IsRequired()
                .HasMaxLength(20);
                
            modelBuilder.Entity<Patient>()
                .Property(p => p.Gender)
                .IsRequired()
                .HasMaxLength(10);
                
            // Configure indexes
            modelBuilder.Entity<Patient>()
                .HasIndex(p => p.PatientCode)
                .IsUnique();
        }
    }
}