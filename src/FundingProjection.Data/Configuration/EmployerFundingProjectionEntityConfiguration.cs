using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.FundingProjection.Domain.Entities;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.FundingProjection.Data.Configuration;

[ExcludeFromCodeCoverage]
internal class EmployerFundingProjectionEntityConfiguration : IEntityTypeConfiguration<EmployerFundingProjectionEntity>
{
    public void Configure(EntityTypeBuilder<EmployerFundingProjectionEntity> builder)
    {
        // Table configuration
        builder.ToTable("EmployerFundingProjection", schema: "dbo");

        // Primary Key
        builder.HasKey(e => e.Id)
            .HasName("PK_EmployerFundingProjection");

        // Column configurations
        builder.Property(e => e.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()")
            .IsRequired();

        builder.Property(e => e.EmployerAccountId)
            .IsRequired();

        builder.Property(e => e.CommittedLearnerCostTotal)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(e => e.CommittedTransferOutTotal)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(e => e.CalendarPeriodMonth)
            .IsRequired();

        builder.Property(e => e.CalendarPeriodYear)
            .IsRequired();

        builder.Property(e => e.LastRecalculatedDate)
            .IsRequired();

        builder.Property(e => e.CreatedDate)
            .HasDefaultValueSql("GETUTCDATE()")
            .IsRequired();

        // Unique Constraint
        builder.HasIndex(e => new { e.EmployerAccountId, e.CalendarPeriodYear, e.CalendarPeriodMonth })
            .IsUnique()
            .HasName("UQ_EmployerFundingProjection_EmployerMonth");

        // Check Constraints (EF Core 5.0+)
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Month_Range",
            "[CalendarPeriodMonth] >= 1 AND [CalendarPeriodMonth] <= 12"));

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Year_Valid",
            "[CalendarPeriodYear] >= 2000"));

        // Additional indexes for query performance
        builder.HasIndex(e => e.EmployerAccountId)
            .HasDatabaseName("IX_EmployerFundingProjection_EmployerAccount")
            .IncludeProperties(e => new { e.CommittedLearnerCostTotal, e.CommittedTransferOutTotal });

        builder.HasIndex(e => new { e.CalendarPeriodYear, e.CalendarPeriodMonth })
            .HasDatabaseName("IX_EmployerFundingProjection_Period")
            .IncludeProperties(e => e.CommittedLearnerCostTotal);
    }
}