using System.ComponentModel.DataAnnotations;
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
    /// Get the import job state for an account, without triggering an import
    /// </summary>
    [HttpGet]
    [Route($"{RouteNames.Jobs}/{{accountId:long}}/{{jobName}}")]
    [ProducesResponseType(typeof(ImportJobStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IResult> GetJobState([FromRoute, Required] long accountId,
        [FromRoute, Required] JobName jobName,
        [FromServices] IImportJobStateRepository repository,
        CancellationToken cancellationToken)
    {
        try
        {
            var jobState = await repository.GetJobState(accountId, jobName, cancellationToken);

            return jobState is null
                ? Results.NotFound()
                : Results.Ok(jobState.MapToResponse());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while getting {JobName} job state for account {AccountId}", jobName, accountId);
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
    [ProducesResponseType(typeof(ImportJobStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IResult> UpdateJobState([FromRoute, Required] Guid id,
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
                AccountId = request.AccountId,
                JobName = request.JobName,
                JobState = request.JobState,
                LastSuccessfulImportDate = request.LastSuccessfulImportDate,
                LastAttemptedDate = request.LastAttemptedDate,
                LastAttemptSuccessful = request.LastAttemptSuccessful,
                TotalRecordsLastRun = request.TotalRecordsLastRun,
                FailedRecordsLastRun = request.FailedRecordsLastRun
            };

            var updatedJobState = await repository.UpdateJobState(entity, cancellationToken);

            return updatedJobState is null 
                ? Results.NotFound() 
                : Results.Ok(updatedJobState.MapToResponse());
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