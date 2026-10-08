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
    public DateTime Dob { get; set; }
    public ApprenticeshipStatus Status { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalPaid { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public decimal? LastPaymentAmount { get; set; }
}