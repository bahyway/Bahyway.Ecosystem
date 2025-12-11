using AlarmInsight.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlarmInsight.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Alarm entity.
/// Configures table, columns, relationships, and value objects.
/// </summary>
internal sealed class AlarmConfiguration : IEntityTypeConfiguration<Alarm>
{
    public void Configure(EntityTypeBuilder<Alarm> builder)
    {
        // Table configuration
        builder.ToTable("alarms");
        builder.HasKey(a => a.Id);

        // Simple properties
        builder.Property(a => a.Id)
            .HasColumnName("id");

        builder.Property(a => a.Source)
            .HasColumnName("source")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.Description)
            .HasColumnName("description")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(a => a.Status)
            .HasColumnName("status")
            .HasConversion<string>() // Store enum as string
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.OccurredAt)
            .HasColumnName("occurred_at")
            .IsRequired();

        builder.Property(a => a.ProcessedAt)
            .HasColumnName("processed_at");

        builder.Property(a => a.ResolvedAt)
            .HasColumnName("resolved_at");

        builder.Property(a => a.Resolution)
            .HasColumnName("resolution")
            .HasMaxLength(2000);

        // Audit properties (from AuditableEntity)
        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(a => a.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.LastModifiedAt)
            .HasColumnName("last_modified_at");

        builder.Property(a => a.LastModifiedBy)
            .HasColumnName("last_modified_by")
            .HasMaxLength(200);

        // Value Object: AlarmSeverity
        // Maps to: severity.Value (int) and severity.Name (string)
        builder.ComplexProperty(a => a.Severity, severity =>
        {
            severity.Property(s => s.Value)
                .HasColumnName("severity_value")
                .IsRequired();

            severity.Property(s => s.Name)
                .HasColumnName("severity_name")
                .HasMaxLength(50)
                .IsRequired();
        });

        // Value Object: Location
        // Maps to: location.Name (string), location.Latitude (double), location.Longitude (double)
        builder.ComplexProperty(a => a.Location, location =>
        {
            location.Property(l => l.Name)
                .HasColumnName("location_name")
                .HasMaxLength(500)
                .IsRequired();

            location.Property(l => l.Latitude)
                .HasColumnName("location_latitude")
                .HasPrecision(9, 6)
                .IsRequired();

            location.Property(l => l.Longitude)
                .HasColumnName("location_longitude")
                .HasPrecision(9, 6)
                .IsRequired();
        });

        // Relationship: Alarm -> AlarmNotes (one-to-many)
        // Use the public property, not the backing field
        builder.HasMany(a => a.Notes)
            .WithOne()
            .HasForeignKey("AlarmId")
            .OnDelete(DeleteBehavior.Cascade)
            .Metadata.PrincipalToDependent!.SetPropertyAccessMode(PropertyAccessMode.Field);

        // Ignore domain events (not persisted)
        builder.Ignore(a => a.DomainEvents);

        // Indexes for performance
        builder.HasIndex(a => a.Status)
            .HasDatabaseName("ix_alarms_status");

        builder.HasIndex(a => a.OccurredAt)
            .HasDatabaseName("ix_alarms_occurred_at");

        builder.HasIndex(a => new { a.Status, a.OccurredAt })
            .HasDatabaseName("ix_alarms_status_occurred_at");
    }
}