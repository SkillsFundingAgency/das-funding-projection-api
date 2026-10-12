using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.FundingProjection.Api.Core;
using SFA.DAS.FundingProjection.Api.Models.Mappers;
using SFA.DAS.FundingProjection.Api.Models.Requests;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Data.Repositories;
using SFA.DAS.FundingProjection.Domain.Enums;
using System.Net;

namespace SFA.DAS.FundingProjection.Api.Controllers;

[ApiController]
public class LearnerController(ILogger<LearnerController> logger) : ControllerBase
{
    [HttpGet]
    [Route($"{RouteNames.Learners}/{RouteElements.Learners}/by/status")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(List<GetCommittedLearnerResponse>), StatusCodes.Status200OK)]
    public async Task<IResult> GetMany(
        [FromQuery] ImportStatus importStatus,
        [FromServices] ICommittedLearnerRepository repository,
        CancellationToken token = default)
    {
        try
        {
            logger.LogInformation("Recruit API: Received request to get committed learners for status: {ImportStatus}", importStatus);

            var result = await repository.GetAllImportStatus(importStatus, token);

            return TypedResults.Ok(result.Select(lr => lr.ToResponse()));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to get committed learners for status: {ImportStatus} : An error occurred", importStatus);
            return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
        }
    }

    [HttpPost]
    [Route($"{RouteNames.Learners}/{{accountId:long}}/{RouteElements.Learners}")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(GetCommittedLearnerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(GetCommittedLearnerResponse), StatusCodes.Status200OK)]
    public async Task<IResult> PostOne(
        [FromRoute] long accountId,
        [FromServices] ICommittedLearnerRepository repository,
        [FromServices] IValidator<PostCommittedLearnerRequest> validator,
        [FromBody] PostCommittedLearnerRequest request,
        CancellationToken token = default)
    {
        try
        {
            var validationResult = await validator.ValidateAsync(request, token);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            logger.LogInformation("Recruit API: Received request to create committed learner for account: {AccountId}", accountId);

            var result = await repository.UpsertOneAsync(request.ToEntity(accountId), token);

            return result.Created
                ? TypedResults.Created($"{result.Entity.Id}", result.Entity.ToResponse())
                : TypedResults.Ok(result.Entity.ToResponse());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to create committed learner for account: {AccountId} : An error occurred", accountId);
            return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
        }
    }

    [HttpPut]
    [Route($"{RouteNames.Learners}/{{accountId:long}}/{RouteElements.Learners}/{{id:guid}}")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(GetCommittedLearnerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(GetCommittedLearnerResponse), StatusCodes.Status200OK)]
    public async Task<IResult> PutOne(
        [FromRoute] Guid id,
        [FromRoute] long accountId,
        [FromServices] ICommittedLearnerRepository repository,
        [FromServices] IValidator<PutCommittedLearnerRequest> validator,
        [FromBody] PutCommittedLearnerRequest request,
        CancellationToken token = default)
    {
        try
        {
            var validationResult = await validator.ValidateAsync(request, token);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            logger.LogInformation("Recruit API: Received request to create committed learner for account: {AccountId}", accountId);

            var result = await repository.UpsertOneAsync(request.ToEntity(id, accountId), token);

            return result.Created
                ? TypedResults.Created($"{result.Entity.Id}", result.Entity.ToResponse())
                : TypedResults.Ok(result.Entity.ToResponse());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to create committed learner for account: {AccountId} : An error occurred", accountId);
            return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
        }
    }
}