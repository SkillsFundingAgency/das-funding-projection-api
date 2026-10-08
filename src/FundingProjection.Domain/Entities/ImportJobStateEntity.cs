using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SFA.DAS.FundingProjection.Domain.Enums;

namespace SFA.DAS.FundingProjection.Domain.Entities;

[Table("ImportJobState", Schema = "dbo")]
public class ImportJobStateEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public JobName JobName { get; set; }

    [Required]
    public DateTime LastSuccessfulImportDate { get; set; }

    [Required]
    public DateTime LastAttemptedDate { get; set; }

    [Required]
    public bool LastAttemptSuccessful { get; set; }

    [Required]
    public int TotalRecordsLastRun { get; set; }

    [Required]
    public int FailedRecordsLastRun { get; set; }

    [Required]
    public DateTime UpdatedDate { get; set; }

    [Required]
    public DateTime CreatedDate { get; set; }
}