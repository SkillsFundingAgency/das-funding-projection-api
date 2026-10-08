using System.ComponentModel.DataAnnotations;
using SFA.DAS.FundingProjection.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.FundingProjection.Domain.Entities;

[Table("CommittedLearner", Schema = "dbo")]
public class CommittedLearnerEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public long EmployerAccountId { get; set; }

    [Required]
    public long ApprenticeshipId { get; set; }

    public long? CommitmentId { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Cost { get; set; }

    [Required]
    [Column(TypeName = "date")]
    public DateOnly StartDate { get; set; }

    [Required]
    [Column(TypeName = "date")]
    public DateOnly EndDate { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(50)")]
    public required PaymentStatus PaymentStatus { get; set; } // 'Active', 'Completed', 'Withdrawn', 'Paused'

    [Required]
    public DateTime CreatedDate { get; set; }

    [Required]
    public DateTime LastUpdatedDate { get; set; }

    [Required]
    public DateTime ImportedDate { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(50)")]
    public ImportStatus ImportStatus { get; set; } // 'Pending', 'Imported', 'Failed'
}