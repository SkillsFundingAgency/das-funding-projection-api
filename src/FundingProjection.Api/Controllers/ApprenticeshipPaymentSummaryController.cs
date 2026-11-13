using Microsoft.AspNetCore.Mvc;
using SFA.DAS.FundingProjection.Api.Core;
using SFA.DAS.FundingProjection.Api.Models.Requests;
using SFA.DAS.FundingProjection.Data.Repositories;
using System.ComponentModel.DataAnnotations;
using SFA.DAS.FundingProjection.Api.Models.Mappers;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Api.Projection;

namespace SFA.DAS.FundingProjection.Api.Controllers;

[ApiController]
[Route($"{RouteNames.ApprenticeshipPaymentSummary}")]
public class ApprenticeshipPaymentSummaryController(ILogger<ApprenticeshipPaymentSummaryController> logger) : ControllerBase
{
    [HttpGet]
    [Route($"{{accountId:long}}/{RouteElements.ApprenticeshipPaymentSummary}")]
    [ProducesResponseType(typeof(List<ApprenticeshipPaymentSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IResult> GetAll(
        [FromServices] IApprenticeshipPaymentSummariesRepository apprenticeshipSummariesRepository,
        [FromRoute, Required] long accountId,
        [FromQuery] DateOnly? awaitingPaymentFor,
        CancellationToken token)
    {
        logger.LogInformation("Processing GET request for apprenticeship payment summaries for account {accountId}", accountId);

        // When a payment period is given, only the summaries still waiting on a payment for it are returned
        var summaries = awaitingPaymentFor is { } paymentPeriod
            ? await apprenticeshipSummariesRepository.GetApprenticeshipPaymentSummariesAwaitingPaymentAsync(accountId, paymentPeriod.ToPeriod(), token)
            : await apprenticeshipSummariesRepository.GetApprenticeshipPaymentSummariesAsync(accountId, token);

        return Results.Ok(summaries.Select(s => s.ToResponse()).ToList());
    }

    [HttpPut]
    [Route($"{{accountId:long}}/{RouteElements.ApprenticeshipPaymentSummary}/{{apprenticeshipId:long}}")]
    [ProducesResponseType(typeof(ApprenticeshipPaymentSummaryResponse), StatusCodes.Status200OK)]
    public async Task<IResult> Put(
        [FromServices] IApprenticeshipPaymentSummariesRepository apprenticeshipSummariesRepository,
        [FromRoute, Required] long accountId,
        [FromRoute, Required] long apprenticeshipId,
        [FromBody, Required] PostApprenticeshipPaymentSummaryRequest request,
        CancellationToken token)
    {
        logger.LogInformation("Processing POST request for apprenticeship payment summary for {apprenticeshipId}", apprenticeshipId);

        var result = await apprenticeshipSummariesRepository.UpsertOneAsync(request.ToEntity(accountId, apprenticeshipId), token);

        return result.Created
            ? Results.Created($"/{RouteNames.ApprenticeshipPaymentSummary}/{apprenticeshipId}", result.Entity.ToResponse())
            : Results.Ok(result.Entity.ToResponse());
    }
}