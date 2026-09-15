using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.FundingProjection.Domain.Entities;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.FundingProjection.Data.Configuration;

[ExcludeFromCodeCoverage]
internal class CommittedLearnerCostEntityConfiguration : IEntityTypeConfiguration<CommittedLearnerCostEntity>
{
    public void Configure(EntityTypeBuilder<CommittedLearnerCostEntity> builder)
    {
        builder.ToTable("CommittedLearnerCost", "dbo");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("Id")
            .HasColumnType("uniqueidentifier")
            .HasDefaultValueSql("NEWSEQUENTIALID()")
            .IsRequired();

        builder.Property(x => x.EmployerAccountId)
            .HasColumnName("EmployerAccountId")
            .HasColumnType("bigint")
            .IsRequired();

        builder.Property(x => x.ApprenticeshipId)
            .HasColumnName("ApprenticeshipId")
            .HasColumnType("bigint")
            .IsRequired();

        builder.Property(x => x.TransferSenderId)
            .HasColumnName("TransferSenderId")
            .HasColumnType("bigint")
            .IsRequired(false);

        builder.Property(x => x.RemainingCost)
            .HasColumnName("RemainingCost")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.PlannedEndDate)
            .HasColumnName("PlannedEndDate")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("Status")
            .HasColumnType("nvarchar(50)")
            .IsRequired();

        builder.Property(x => x.LastUpdatedDate)
            .HasColumnName("LastUpdatedDate")
            .HasColumnType("datetime2")
            .IsRequired();

        // Unique constraint
        builder.HasIndex(x => new {x.EmployerAccountId, x.ApprenticeshipId})
            .IsUnique()
            .HasDatabaseName("UQ_CommittedLearnerCost_Account_Apprenticeship");

        // Non-clustered index with INCLUDE columns
        builder.HasIndex(x => x.EmployerAccountId)
            .HasDatabaseName("IX_CommittedLearnerCost_EmployerAccountId")
            .IncludeProperties(x => new {x.RemainingCost, x.TransferSenderId, x.Status});
    }
}