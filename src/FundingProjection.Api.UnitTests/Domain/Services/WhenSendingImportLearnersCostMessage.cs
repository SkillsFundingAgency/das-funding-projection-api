using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using SFA.DAS.FundingProjection.Domain.Events;
using SFA.DAS.FundingProjection.Domain.Services;

namespace SFA.DAS.FundingProjection.Api.UnitTests.Domain.Services;

public class WhenSendingImportLearnersCostMessage
{
    [Test, MoqAutoData]
    public async Task Then_The_Message_Is_Sent_To_The_Funding_Projection_Queue(Guid jobId, long accountId)
    {
        ServiceBusMessage? sentMessage = null;
        var sender = new Mock<ServiceBusSender>();
        sender
            .Setup(x => x.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()))
            .Callback<ServiceBusMessage, CancellationToken>((m, _) => sentMessage = m)
            .Returns(Task.CompletedTask);
        var client = new Mock<ServiceBusClient>();
        client.Setup(x => x.CreateSender(QueueConstants.FundingProjectionQueue)).Returns(sender.Object);

        var sut = new EventService(Mock.Of<ILogger<EventService>>(), client.Object);

        await sut.HandleImportLearnersCost(jobId, accountId);

        sentMessage.Should().NotBeNull();
        JsonSerializer.Deserialize<ImportLearnersCostEvent>(sentMessage!.Body.ToString())
            .Should().Be(new ImportLearnersCostEvent(jobId, accountId));
    }
}
