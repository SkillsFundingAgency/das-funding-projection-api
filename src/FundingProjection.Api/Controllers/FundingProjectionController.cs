using Microsoft.AspNetCore.Mvc;
using SFA.DAS.FundingProjection.Api.Core;
using SFA.DAS.FundingProjection.Api.Models;
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
    private const int MinRequiredProjectionMonths = 12;

    [HttpPost]
    [Route($"{{accountId:long}}/{RouteElements.FundingProjection}")]
    [ProducesResponseType(typeof(EstimatesTimeline), StatusCodes.Status200OK)]
    public async Task<IResult> PostEmployerFundingProjection(
        [FromServices] IApprenticeshipPaymentSummariesRepository apprenticeshipSummariesRepository,
        [FromServices] IImportJobStateRepository importJobStateRepository,
        [FromServices] IEventService eventService,
        [FromServices] ITimelineGenerator timelineGenerator,
        [FromRoute] [Required] long accountId,
        [FromBody] [Required] PostEmployerFundingProjectionRequest request,
        CancellationToken token = default)
    {
        logger.LogInformation("Funding Projection API: Received query to get projection by accountId : {Id}", accountId);

        var startingPeriod = (request.StartPeriod ?? DateOnly.FromDateTime(DateTime.UtcNow)).ToPeriod();
        var apprenticeshipSummaries = await apprenticeshipSummariesRepository.GetApprenticeshipPaymentSummariesAsync(accountId, token);
        var timelineData = new TimelineData
        {
            AccountId = accountId,
            StartingPeriod = startingPeriod,
            OpeningBalance = request.CurrentLevyFunds,
            Months = Math.Clamp(request.Months, 1, MaxProjectionMonths),
            HistoricLevyIn = request.HistoricLevyIn,
            ApprenticeshipPaymentSummaries = apprenticeshipSummaries,
            LevyFundsInForExpiry = request.LevyFundsInForExpiry,
            PaymentFundsOutForExpiry = request.PaymentFundsOutForExpiry,
            ShortTermExpiredFunds = request.ShortTermExpiredFunds,
            LongTermExpiredFunds = request.LongTermExpiredFunds,
            CurrentPeriodLevyInReceived = request.HistoricLevyIn.Any(x => x.Period.ToPeriod() == startingPeriod),
            CurrentPeriodPayments = startingPeriod == DateOnly.FromDateTime(DateTime.UtcNow).ToPeriod()
                ? CurrentPeriodPayments.From(request.CurrentPayments)
                : null
        };
        var timeline = timelineGenerator.Create(timelineData);
        
        var job = await importJobStateRepository.GetOrCreateJobStateAsync(accountId, JobName.ImportLearnersCost, token);
        await eventService.HandleImportLearnersCost(job.Id, accountId, job.LastSuccessfulImportDate);

        return TypedResults.Ok(timeline);
    }

    [HttpGet]
    [Route($"{{accountId:long}}/{RouteElements.FundingProjection}/{RouteElements.Months}")]
    [ProducesResponseType(typeof(ProjectionMonthsResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetProjectionMonths(
        [FromServices] IApprenticeshipPaymentSummariesRepository apprenticeshipSummariesRepository,
        [FromRoute] [Required] long accountId,
        CancellationToken token = default)
    {
        logger.LogInformation("Funding Projection API: Received query to get projection months by accountId : {Id}", accountId);

        var apprenticeshipSummaries = await apprenticeshipSummariesRepository.GetApprenticeshipPaymentSummariesAsync(accountId, token);

        var lastPaymentPeriod = apprenticeshipSummaries
            .Where(x => x.CanBeProjected)
            .Max(x => (DateOnly?)x.LastPaymentPeriod);

        var startingPeriod = DateOnly.FromDateTime(DateTime.UtcNow).ToPeriod();
        var months = lastPaymentPeriod is { } last
            ? (last.Year - startingPeriod.Year) * 12 + last.Month - startingPeriod.Month + 1
            : 0;

        return TypedResults.Ok(new ProjectionMonthsResponse
        {
            Months = Math.Clamp(months, MinRequiredProjectionMonths, MaxProjectionMonths)
        });
    }
}