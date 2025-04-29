using System;
using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PharmaForms.Core.Entities;

namespace PharmaForms.Infrastructure.Data
{
    /// <summary>
    /// The main database context for the application.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<FormDefinition> FormDefinitions { get; set; }
        public DbSet<FormSubmission> FormSubmissions { get; set; }
        public DbSet<FormDependency> FormDependencies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all configurations from assembly
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Configure entity relationships
            ConfigureFormDefinition(modelBuilder);
            ConfigureFormSubmission(modelBuilder);
            ConfigureFormDependency(modelBuilder);
        }

        private void ConfigureFormDefinition(ModelBuilder modelBuilder)
        {
            // Configure the FormDefinition entity
            modelBuilder.Entity<FormDefinition>(entity =>
            {
                entity.ToTable("FormDefinitions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Version).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Direction).IsRequired().HasMaxLength(3);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);

                // Serialize complex types to JSON strings
                entity.Property(e => e.Sections)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                        v => JsonSerializer.Deserialize<List<FormSection>>(v, (JsonSerializerOptions)null))
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.Metadata)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                        v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null))
                    .HasColumnType("nvarchar(max)");
            });
        }

        private void ConfigureFormSubmission(ModelBuilder modelBuilder)
        {
            // Configure the FormSubmission entity
            modelBuilder.Entity<FormSubmission>(entity =>
            {
                entity.ToTable("FormSubmissions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.FormId).IsRequired().HasMaxLength(36);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.Property(e => e.ApprovedBy).HasMaxLength(100);
                entity.Property(e => e.RejectedBy).HasMaxLength(100);
                entity.Property(e => e.Comments).HasMaxLength(1000);

                // Configure relationship with FormDefinition
                entity.HasOne(e => e.Form)
                    .WithMany()
                    .HasForeignKey(e => e.FormId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Configure JSON conversion for the Data property
                entity.Property(e => e.Data)
                    .HasConversion(
                        v => v.RootElement.GetRawText(),
                        v => JsonDocument.Parse(v),
                        new ValueComparer<JsonDocument>(
                            (l, r) => l.RootElement.GetRawText() == r.RootElement.GetRawText(),
                            v => v.RootElement.GetRawText().GetHashCode(),
                            v => JsonDocument.Parse(v.RootElement.GetRawText())))
                    .HasColumnType("nvarchar(max)");
            });
        }

        private void ConfigureFormDependency(ModelBuilder modelBuilder)
        {
            // Configure the FormDependency entity
            modelBuilder.Entity<FormDependency>(entity =>
            {
                entity.ToTable("FormDependencies");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.SourceFormId).IsRequired().HasMaxLength(36);
                entity.Property(e => e.SourceFieldId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TargetFormId).IsRequired().HasMaxLength(36);
                entity.Property(e => e.TargetFieldId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DependencyType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Expression).HasMaxLength(1000);
                entity.Property(e => e.LookupKey).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);

                // Create indexes for faster lookups
                entity.HasIndex(e => new { e.SourceFormId, e.SourceFieldId });
                entity.HasIndex(e => new { e.TargetFormId, e.TargetFieldId });
                entity.HasIndex(e => e.DependencyType);
            });
        }
    }
}
