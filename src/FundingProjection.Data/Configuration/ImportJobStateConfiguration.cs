using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.FundingProjection.Domain.Entities;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.FundingProjection.Data.Configuration;

[ExcludeFromCodeCoverage]
public class ImportJobStateConfiguration : IEntityTypeConfiguration<ImportJobStateEntity>
{
    public void Configure(EntityTypeBuilder<ImportJobStateEntity> builder)
    {
        // Table configuration
        builder.ToTable("ImportJobState", schema: "dbo");

        // Primary Key
        builder.HasKey(e => e.Id)
            .HasName("PK_ImportJobState");

        // Column configurations
        builder.Property(e => e.Id)
            .HasColumnType("uniqueidentifier")
            .HasDefaultValueSql("NEWSEQUENTIALID()")
            .IsRequired();

        builder.Property(e => e.JobName)
            .HasColumnType("nvarchar(100)")
            .IsRequired();

        builder.Property(e => e.LastSuccessfulImportDate)
            .IsRequired();

        builder.Property(e => e.LastAttemptedDate)
            .IsRequired();

        builder.Property(e => e.LastAttemptSuccessful)
            .HasColumnType("bit")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(e => e.TotalRecordsLastRun)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(e => e.FailedRecordsLastRun)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(e => e.UpdatedDate)
            .HasDefaultValueSql("GETUTCDATE()")
            .IsRequired();

        builder.Property(e => e.CreatedDate)
            .HasDefaultValueSql("GETUTCDATE()")
            .IsRequired();

        // Unique Constraint
        builder.HasIndex(e => e.JobName)
            .IsUnique()
            .HasDatabaseName("UQ_ImportJobState_JobName");

        // Check Constraints
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_JobName_NotEmpty",
            "LEN([JobName]) > 0"));

        // Additional indexes for query performance
        builder.HasIndex(e => e.JobName)
            .HasDatabaseName("IX_ImportJobState_JobName")
            .IncludeProperties(e => new { e.LastSuccessfulImportDate, e.LastAttemptSuccessful });

        builder.HasIndex(e => e.LastAttemptedDate)
            .HasDatabaseName("IX_ImportJobState_LastAttempt")
            .IsDescending(true);
    }
}
