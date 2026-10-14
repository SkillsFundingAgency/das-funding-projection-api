using NServiceBus;
using Microsoft.Extensions.Logging;
using SFA.DAS.FundingProjection.Domain.Events;

namespace SFA.DAS.FundingProjection.Domain.Services;

public interface IEventService
{
    Task HandleImportLearnersCost(Guid jobId, long accountId);
}
public class EventService(ILogger<EventService> logger, IMessageSession messageSession) : IEventService
{
    public async Task HandleImportLearnersCost(Guid jobId, long accountId)
    {
        logger.LogInformation("Publishing ImportLearnerCost event for account {AccountId}.", accountId);
        await messageSession.Publish(new ImportLearnersCostEvent(jobId, accountId));
    }
}