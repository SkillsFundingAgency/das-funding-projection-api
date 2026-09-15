using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.FundingProjection.Domain.Entities;

[Table("EmployerFundingProjection", Schema = "dbo")]
public class EmployerFundingProjectionEntity
{
    [Key]
    [Column(TypeName = "bigint")]
    public long EmployerAccountId { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal CommittedLearnerCostTotal { get; set; } = 0;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal CommittedTransferOutTotal { get; set; } = 0;

    [Required]
    public DateTime LastRecalculatedDate { get; set; }

    [Required]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}