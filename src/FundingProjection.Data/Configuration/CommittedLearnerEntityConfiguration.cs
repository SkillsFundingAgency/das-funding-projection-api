using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.FundingProjection.Domain.Entities;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.FundingProjection.Data.Configuration;

[ExcludeFromCodeCoverage]
internal class CommittedLearnerEntityConfiguration : IEntityTypeConfiguration<CommittedLearnerEntity>
{
    public void Configure(EntityTypeBuilder<CommittedLearnerEntity> builder)
    {
        builder.ToTable("CommittedLearners", "dbo");
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

        builder.Property(x => x.CommitmentId)
            .HasColumnName("CommitmentId")
            .HasColumnType("bigint")
            .IsRequired(false);

        builder.Property(x => x.Cost)
            .HasColumnName("Cost")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.StartDate)
            .HasColumnName("StartDate")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.EndDate)
            .HasColumnName("EndDate")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.PaymentStatus)
            .HasColumnName("PaymentStatus")
            .HasColumnType("nvarchar(50)")
            .IsRequired();

        builder.Property(x => x.CreatedDate)
            .HasColumnName("CreatedDate")
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.LastUpdatedDate)
            .HasColumnName("LastUpdatedDate")
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.ImportedDate)
            .HasColumnName("ImportedDate")
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.ImportStatus)
            .HasColumnName("ImportStatus")
            .HasColumnType("nvarchar(50)")
            .IsRequired();

        // Unique constraint
        builder.HasIndex(x => new {x.EmployerAccountId, x.ApprenticeshipId})
            .IsUnique()
            .HasDatabaseName("UQ_CommittedLearner_Account_Apprenticeship");

        // Non-clustered index with INCLUDE columns
        builder.HasIndex(x => x.EmployerAccountId)
            .HasDatabaseName("IX_CommittedLearner_EmployerAccountId")
            .IncludeProperties(x => new {x.Cost, x.PaymentStatus});
    }
}