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
        builder.ToTable("EmployerFundingProjection");
        builder.HasKey(x => x.EmployerAccountId);

        builder.Property(x => x.EmployerAccountId)
            .HasColumnName("EmployerAccountId")
            .HasColumnType("bigint")
            .IsRequired();
            
        builder.Property(e => e.CommittedLearnerCostTotal)
            .HasColumnName("CommittedLearnerCostTotal")
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.Property(e => e.CommittedTransferOutTotal)
            .HasColumnName("CommittedTransferOutTotal")
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.Property(e => e.CreatedDate)
            .HasColumnName("CreatedDate")
            .HasColumnType("datetime")
            .HasDefaultValueSql("GETUTCDATE()");
    }
}