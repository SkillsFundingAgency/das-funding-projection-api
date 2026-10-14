using Microsoft.AspNetCore.Mvc;
using SFA.DAS.FundingProjection.Api.Core;
using SFA.DAS.FundingProjection.Api.Models.Requests;
using SFA.DAS.FundingProjection.Data.Repositories;
using System.ComponentModel.DataAnnotations;
using SFA.DAS.FundingProjection.Api.Models.Mappers;
using SFA.DAS.FundingProjection.Api.Models.Responses;

namespace SFA.DAS.FundingProjection.Api.Controllers;

[ApiController]
[Route($"{RouteNames.ApprenticeshipPaymentSummary}")]
public class ApprenticeshipPaymentSummaryController(ILogger<ApprenticeshipPaymentSummaryController> logger) : ControllerBase
{
    [HttpPost]
    [Route($"{{accountId:long}}/{RouteElements.ApprenticeshipPaymentSummary}/{{apprenticeshipId:long}}")]
    [ProducesResponseType(typeof(ApprenticeshipPaymentSummaryResponse), StatusCodes.Status200OK)]
    public async Task<IResult> Post(
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