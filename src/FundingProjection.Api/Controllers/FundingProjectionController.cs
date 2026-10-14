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
    [HttpPost]
    [Route($"{{accountId:long}}/{RouteElements.FundingProjection}")]
    [ProducesResponseType(typeof(EstimatesTimeline), StatusCodes.Status200OK)]
    public async Task<IResult> PostEmployerFundingProjection(
        [FromServices] IEmployerFundingProjectionRepository repository,
        [FromRoute] [Required] long accountId,
        [FromBody] PostEmployerFundingProjectionRequest request,
        CancellationToken token = default)
    {
        logger.LogInformation("Funding Projection API: Received query to get projection by accountId : {Id}", accountId);
        var now = DateOnly.FromDateTime(DateTime.UtcNow).ToPeriod();
        var cappedMonths = Math.Min(request.Months, 120);
        var apprenticeshipSummaries = await repository.GetApprenticeshipSummariesAsync(accountId);
        var commitmentProjections = CommitmentsProjector.CreateProjection(now, cappedMonths, [.. apprenticeshipSummaries.Where(x => !x.IsTransfer)]);
        var transferOutProjections = CommitmentsProjector.CreateProjection(now, cappedMonths, [.. apprenticeshipSummaries.Where(x => x.IsTransfer)]);
        var levyInProjections = LevyInProjector.CreateProjection(now, cappedMonths, request.HistoricLevyIn);

        var timeline = EstimatesTimeline.From(
            accountId,
            now,
            cappedMonths,
            0m,
            commitmentProjections,
            transferOutProjections,
            levyInProjections);
        return TypedResults.Ok(timeline);
    }
}