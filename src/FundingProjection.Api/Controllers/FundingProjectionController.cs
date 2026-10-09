using Microsoft.AspNetCore.Mvc;
using SFA.DAS.FundingProjection.Data.Repositories;
using System.ComponentModel.DataAnnotations;
using System.Net;
using SFA.DAS.FundingProjection.Api.Core;
using SFA.DAS.FundingProjection.Api.Models.Mappers;
using SFA.DAS.FundingProjection.Api.Models.Requests;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Api.Projection;
using SFA.DAS.FundingProjection.Data.Services;

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
        
        var apprenticeshipSummaries = await repository.GetApprenticeshipSummariesAsync(accountId);
        var commitmentProjections = CommitmentsProjector.CreateProjection(now, request.Months, apprenticeshipSummaries);
        var levyInProjections = LevyInProjector.CreateProjection(now, request.Months, request.HistoricLevyIn);

        var timeline = EstimatesTimeline.From(
            accountId,
            now,
            request.Months,
            0m,
            commitmentProjections,
            levyInProjections);
        return TypedResults.Ok(timeline);
    }

    [HttpPost]
    [Route($"{RouteElements.FundingProjection}/re-calculate")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RecalculateFundingProjectionResponse), StatusCodes.Status200OK)]
    public async Task<IResult> RecalculateFundingProjection(
        [FromBody] DateTime cutOffDateTime,
        [FromServices] IFundingProjectionServices fundingProjectionServices,
        CancellationToken token)
    {
        try
        {
            logger.LogInformation("Funding Projection API: Received request to re-calculate projection");

            var result = await fundingProjectionServices.UpdateProjection(cutOffDateTime, token);

            return Results.Ok(result.ToRecalculateFundingProjectionResponse());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to re-calculate funding projection : An error occurred");
            return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
        }
    }
}