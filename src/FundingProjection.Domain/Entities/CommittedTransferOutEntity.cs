using SFA.DAS.FundingProjection.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.FundingProjection.Domain.Entities;

[Table("CommittedTransferOut", Schema = "dbo")]
public class CommittedTransferOutEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public long TransferSenderId { get; set; }

    [Required]
    public long ApprenticeshipId { get; set; }

    public int? PledgeApplicationId { get; set; }

    [Required]
    public byte TransferApprovalStatus { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal RemainingValue { get; set; }

    [Required]
    [Column(TypeName = "date")]
    public DateOnly PlannedEndDate { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(50)")]
    public TransferOutStatus Status { get; set; }

    [Required]
    public DateTime LastUpdatedDate { get; set; }
}