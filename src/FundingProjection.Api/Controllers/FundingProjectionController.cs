using Microsoft.AspNetCore.Mvc;
using SFA.DAS.FundingProjection.Api.Core;
using SFA.DAS.FundingProjection.Api.Models.Requests;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Api.Projection;
using SFA.DAS.FundingProjection.Data.Repositories;
using System.ComponentModel.DataAnnotations;
using SFA.DAS.FundingProjection.Domain.Enums;
using SFA.DAS.FundingProjection.Domain.Services;

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
        [FromServices] IApprenticeshipPaymentSummariesRepository apprenticeshipSummariesRepository,
        [FromServices] IImportJobStateRepository importJobStateRepository,
        [FromServices] IEventService eventService,
        [FromServices] TimelineGenerator timelineGenerator,
        [FromRoute] [Required] long accountId,
        [FromBody] [Required] PostEmployerFundingProjectionRequest request,
        CancellationToken token = default)
    {
        logger.LogInformation("Funding Projection API: Received query to get projection by accountId : {Id}", accountId);
        
        var apprenticeshipSummaries = await apprenticeshipSummariesRepository.GetApprenticeshipPaymentSummariesAsync(accountId, token);
        var timelineData = new TimelineData
        {
            AccountId = accountId,
            StartingPeriod = (request.StartPeriod ?? DateOnly.FromDateTime(DateTime.UtcNow)).ToPeriod(),
            OpeningBalance = request.CurrentLevyFunds,
            Months = Math.Clamp(request.Months, 1, MaxProjectionMonths),
            HistoricLevyIn = request.HistoricLevyIn,
            ApprenticeshipPaymentSummaries = apprenticeshipSummaries,
            LevyFundsInForExpiry = request.LevyFundsInForExpiry,
            PaymentFundsOutForExpiry = request.PaymentFundsOutForExpiry,
            ShortTermExpiredFunds = request.ShortTermExpiredFunds,
            LongTermExpiredFunds = request.LongTermExpiredFunds,
        };
        var timeline = timelineGenerator.Create(timelineData);
        
        var job = await importJobStateRepository.GetOrCreateJobStateAsync(accountId, JobName.ImportLearnersCost, token);
        await eventService.HandleImportLearnersCost(job.Id, accountId);

        return TypedResults.Ok(timeline);
    }
}