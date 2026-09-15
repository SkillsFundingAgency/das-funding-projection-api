using System.ComponentModel.DataAnnotations;
using SFA.DAS.FundingProjection.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.FundingProjection.Domain.Entities;

[Table("CommittedLearnerCost", Schema = "dbo")]
public class CommittedLearnerCostEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public long EmployerAccountId { get; set; }

    [Required]
    public long ApprenticeshipId { get; set; }

    public long? TransferSenderId { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal RemainingCost { get; set; }

    [Required]
    [Column(TypeName = "date")]
    public DateOnly PlannedEndDate { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(50)")]
    public LearnerCostStatus Status { get; set; }

    [Required]
    public DateTime LastUpdatedDate { get; set; }
}