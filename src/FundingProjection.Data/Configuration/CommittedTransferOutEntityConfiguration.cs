using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.FundingProjection.Domain.Entities;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.FundingProjection.Data.Configuration;

[ExcludeFromCodeCoverage]
internal class CommittedTransferOutEntityConfiguration : IEntityTypeConfiguration<CommittedTransferOutEntity>
{
    public void Configure(EntityTypeBuilder<CommittedTransferOutEntity> builder)
    {
        builder.ToTable("CommittedTransferOut", "dbo");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("Id")
            .HasColumnType("uniqueidentifier")
            .HasDefaultValueSql("NEWSEQUENTIALID()")
            .IsRequired();

        builder.Property(x => x.TransferSenderId)
            .HasColumnName("TransferSenderId")
            .HasColumnType("bigint")
            .IsRequired();

        builder.Property(x => x.ApprenticeshipId)
            .HasColumnName("ApprenticeshipId")
            .HasColumnType("bigint")
            .IsRequired();

        builder.Property(x => x.PledgeApplicationId)
            .HasColumnName("PledgeApplicationId")
            .HasColumnType("int")
            .IsRequired(false);

        builder.Property(x => x.TransferApprovalStatus)
            .HasColumnName("TransferApprovalStatus")
            .HasColumnType("tinyint")
            .IsRequired();

        builder.Property(x => x.RemainingValue)
            .HasColumnName("RemainingValue")
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

        builder.HasIndex(x => new { x.TransferSenderId, x.ApprenticeshipId })
            .IsUnique()
            .HasDatabaseName("UQ_CommittedTransferOut_Sender_Apprenticeship");

        builder.HasIndex(x => x.TransferSenderId)
            .HasDatabaseName("IX_CommittedTransferOut_TransferSenderId")
            .IncludeProperties(x => new { x.RemainingValue, x.TransferApprovalStatus, x.PledgeApplicationId, x.Status });
    }
}