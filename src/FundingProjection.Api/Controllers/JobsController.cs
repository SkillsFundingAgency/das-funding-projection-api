using Microsoft.AspNetCore.Mvc;
using SFA.DAS.FundingProjection.Api.Core;
using SFA.DAS.FundingProjection.Api.Models.Mappers;
using SFA.DAS.FundingProjection.Api.Models.Requests;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Data.Repositories;
using SFA.DAS.FundingProjection.Domain.Entities;
using SFA.DAS.FundingProjection.Domain.Enums;

namespace SFA.DAS.FundingProjection.Api.Controllers;

[ApiController]
public class JobsController(ILogger<JobsController> logger) : ControllerBase
{
    /// <summary>
    /// Get or create import job state
    /// Returns the job state with LastSuccessfulImportDate to use as cutoff for import
    /// </summary>
    [HttpPost]
    [Route($"{RouteNames.Jobs}/{{jobName}}")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ImportJobStateResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetOrCreateJobState(
        [FromRoute] JobName jobName,
        [FromServices] IImportJobStateRepository repository,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Getting or creating job state for {JobName}", jobName);
            var jobState = await repository.GetOrCreateJobStateAsync(jobName, cancellationToken);
            return Results.Ok(jobState.MapToResponse());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing the request");
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Update import job state with results from import run
    /// </summary>
    [HttpPut]
    [Route($"{RouteNames.Jobs}/{{id:guid}}")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ImportJobStateResponse), StatusCodes.Status200OK)]
    public async Task<IResult> UpdateJobState([FromRoute] Guid id,
        [FromBody] PutImportJobStateRequest request,
        [FromServices] IImportJobStateRepository repository,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Updating job state for ID {Id}", id);
            var entity = new ImportJobStateEntity
            {
                Id = id,
                LastSuccessfulImportDate = request.LastSuccessfulImportDate,
                LastAttemptedDate = request.LastAttemptedDate,
                LastAttemptSuccessful = request.LastAttemptSuccessful,
                TotalRecordsLastRun = request.TotalRecordsLastRun,
                FailedRecordsLastRun = request.FailedRecordsLastRun
            };

            var updatedJobState = await repository.UpdateJobStateAsync(entity, cancellationToken);
            return Results.Ok(updatedJobState.MapToResponse());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating job state for ID {Id}", id);
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}