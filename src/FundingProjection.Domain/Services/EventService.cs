using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using SFA.DAS.FundingProjection.Domain.Events;

namespace SFA.DAS.FundingProjection.Domain.Services;

public interface IEventService
{
    Task HandleImportLearnersCost(Guid jobId, long accountId);
}
public class EventService(ILogger<EventService> logger, ServiceBusClient serviceBusClient) : IEventService
{
    public async Task HandleImportLearnersCost(Guid jobId, long accountId)
    {
        logger.LogInformation("Sending ImportLearnerCost message for account {AccountId}.", accountId);
        await using var sender = serviceBusClient.CreateSender(QueueConstants.FundingProjectionQueue);
        var body = JsonSerializer.Serialize(new ImportLearnersCostEvent(jobId, accountId));
        await sender.SendMessageAsync(new ServiceBusMessage(body) { ContentType = "application/json" });
    }
}
