using Microsoft.AspNetCore.Mvc;
using SFA.DAS.FundingProjection.Api.Core;
using SFA.DAS.FundingProjection.Api.Models.Mappers;
using SFA.DAS.FundingProjection.Api.Models.Requests;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Api.Projection;
using SFA.DAS.FundingProjection.Data.Repositories;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.FundingProjection.Api.Controllers;

[ApiController]
[Route($"{RouteNames.EmployerFundingProjection}")]
public class FundingProjectionController(ILogger<FundingProjectionController> logger) : ControllerBase
{
    private const int MaxProjectionMonths = 120;

    [HttpPost]
    [Route($"{{accountId:long}}/{RouteElements.FundingProjection}")]
    [ProducesResponseType(typeof(EstimatesTimeline), StatusCodes.Status200OK)]
    public async Task<IResult> PostEmployerFundingProjection(
        [FromServices] IEmployerFundingProjectionRepository repository,
        [FromRoute] [Required] long accountId,
        [FromBody] [Required] PostEmployerFundingProjectionRequest request,
        CancellationToken token = default)
    {
        logger.LogInformation("Funding Projection API: Received query to get projection by accountId : {Id}", accountId);
        
        var now = DateOnly.FromDateTime(DateTime.UtcNow).ToPeriod();
        var cappedMonths = Math.Clamp(request.Months, 1, MaxProjectionMonths);

        var apprenticeshipSummaries = await repository.GetApprenticeshipSummariesAsync(accountId, token);

        var payments = apprenticeshipSummaries.Where(x => !x.IsTransfer).ToList();
        var transfers = apprenticeshipSummaries.Where(x => x.IsTransfer).ToList();

        var commitmentProjections = CommitmentsProjector.CreateProjection(now, cappedMonths, payments);
        var transferOutProjections = CommitmentsProjector.CreateProjection(now, cappedMonths, transfers);
        var levyInProjections = LevyInProjector.CreateProjection(now, cappedMonths, request.HistoricLevyIn);

        var timeline = EstimatesTimeline.From(
            accountId,
            now,
            cappedMonths,
            openingBalance: 0m,
            commitmentProjections,
            transferOutProjections,
            levyInProjections);

        return TypedResults.Ok(timeline);
    }
}