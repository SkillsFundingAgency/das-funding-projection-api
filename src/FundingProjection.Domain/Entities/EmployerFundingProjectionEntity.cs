using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.FundingProjection.Domain.Entities;

[Table("EmployerFundingProjection", Schema = "dbo")]
public class EmployerFundingProjectionEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public long EmployerAccountId { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal CommittedLearnerCostTotal { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal CommittedTransferOutTotal { get; set; }

    [Required]
    public int CalendarPeriodMonth { get; set; }

    [Required]
    public int CalendarPeriodYear { get; set; }

    [Required]
    public DateTime LastRecalculatedDate { get; set; }

    [Required]
    public DateTime CreatedDate { get; set; }
}