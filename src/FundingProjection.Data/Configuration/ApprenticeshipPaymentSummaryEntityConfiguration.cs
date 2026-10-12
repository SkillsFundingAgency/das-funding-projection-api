using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.FundingProjection.Domain.Entities;
using SFA.DAS.FundingProjection.Domain.Enums;

namespace SFA.DAS.FundingProjection.Data.Configuration;

public class ApprenticeshipPaymentSummaryEntityConfiguration: IEntityTypeConfiguration<ApprenticeshipPaymentSummaryEntity> 
{
    public void Configure(EntityTypeBuilder<ApprenticeshipPaymentSummaryEntity> builder)
    {
        builder.ToTable("ApprenticeshipPaymentSummaries", "dbo");
        builder.HasKey(x => x.ApprenticeshipId);

        builder.Property(x => x.ApprenticeshipId)
            .HasColumnName("ApprenticeshipId")
            .HasColumnType("BIGINT")
            .IsRequired();

        builder.Property(x => x.AccountId)
            .HasColumnName("AccountId")
            .HasColumnType("BIGINT")
            .IsRequired();

        builder.Property(x => x.Uln)
            .HasColumnName("Uln")
            .HasColumnType("BIGINT")
            .IsRequired();

        builder.Property(x => x.Dob)
            .HasColumnName("Dob")
            .HasColumnType("DATE")
            .IsRequired(false);

        builder.Property(x => x.Status)
            .HasColumnName("Status")
            .HasColumnType("NVARCHAR(30)")
            .HasConversion(v => v.ToString(), v => Enum.Parse<ApprenticeshipStatus>(v))
            .IsRequired();

        builder.Property(x => x.TotalCost)
            .HasColumnName("TotalCost")
            .HasColumnType("DECIMAL(18,2)")
            .IsRequired();
        
        builder.Property(x => x.TotalPaid)
            .HasColumnName("TotalPaid")
            .HasColumnType("DECIMAL(18,2)")
            .IsRequired();

        builder.Property(x => x.StartDate)
            .HasColumnName("StartDate")
            .HasColumnType("DATE")
            .IsRequired();

        builder.Property(x => x.EndDate)
            .HasColumnName("EndDate")
            .HasColumnType("DATE")
            .IsRequired();

        builder.Property(x => x.LastPaymentDate)
            .HasColumnName("LastPaymentDate")
            .HasColumnType("DATE")
            .IsRequired(false);
        
        builder.Property(x => x.LastPaymentAmount)
            .HasColumnName("LastPaymentAmount")
            .HasColumnType("DECIMAL(18,2)")
            .IsRequired(false);
        
        builder.Property(x => x.LastUpdatedDate)
            .HasColumnName("LastUpdatedDate")
            .HasColumnType("DATETIME")
            .IsRequired(false);
        
        // Non-clustered index
        builder.HasIndex(x => x.AccountId)
            .HasDatabaseName("IX_ApprenticeshipPaymentSummary_AccountId");
    }
}