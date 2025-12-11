using AlarmInsight.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlarmInsight.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for AlarmNote entity.
/// </summary>
internal sealed class AlarmNoteConfiguration : IEntityTypeConfiguration<AlarmNote>
{
    public void Configure(EntityTypeBuilder<AlarmNote> builder)
    {
        // Table configuration
        builder.ToTable("alarm_notes");
        builder.HasKey(n => n.Id);

        // Properties
        builder.Property(n => n.Id)
            .HasColumnName("id");

        builder.Property<int>("AlarmId")
            .HasColumnName("alarm_id")
            .IsRequired();

        builder.Property(n => n.Content)
            .HasColumnName("content")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(n => n.Author)
            .HasColumnName("author")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(n => n.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Foreign key index
        builder.HasIndex("AlarmId")
            .HasDatabaseName("ix_alarm_notes_alarm_id");
    }
}