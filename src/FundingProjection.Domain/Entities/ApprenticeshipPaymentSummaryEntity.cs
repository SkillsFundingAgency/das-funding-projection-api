using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SFA.DAS.FundingProjection.Domain.Enums;

namespace SFA.DAS.FundingProjection.Domain.Entities;

[Table("ApprenticeshipPaymentSummary", Schema = "dbo")]
public class ApprenticeshipPaymentSummaryEntity
{
    [Key]
    public long ApprenticeshipId { get; set; }
    public long AccountId { get; set; }
    public long Uln { get; set; }
    public DateOnly? Dob { get; set; }
    public ApprenticeshipStatus Status { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCost { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPaid { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateOnly? LastPaymentDate { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal? LastPaymentAmount { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
}