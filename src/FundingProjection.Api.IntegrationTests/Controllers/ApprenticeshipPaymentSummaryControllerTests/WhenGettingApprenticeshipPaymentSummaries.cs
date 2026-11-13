using System.Collections.Generic;
using System.Linq;
using FundingProjection.Api.Testing;
using FundingProjection.Api.Testing.DataCreation;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Api.Projection;
using SFA.DAS.FundingProjection.Domain.Entities;

namespace FundingProjection.Api.IntegrationTests.Controllers.ApprenticeshipPaymentSummaryControllerTests;

public class WhenGettingApprenticeshipPaymentSummaries : SqlBaseFixture
{
    private static Uri GetUrl(long accountId, DateOnly? awaitingPaymentFor = null) => new(
        $"api/employer/{accountId}/apprenticeship-payment-summary"
        + (awaitingPaymentFor is { } period ? $"?awaitingPaymentFor={period:yyyy-MM-dd}" : string.Empty),
        UriKind.Relative);

    [Test]
    public async Task Then_All_Of_The_Accounts_Summaries_Are_Returned()
    {
        // arrange
        var accountId = AccountIdGenerator.Next();
        var first = await DbData.AddAsync(ApprenticeshipPaymentSummaryEntity.Create(accountId));
        var second = await DbData.AddAsync(ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x.TotalPaid(TestData.DefaultTotalCost)));
        await DbData.AddAsync(ApprenticeshipPaymentSummaryEntity.Create(AccountIdGenerator.Next()));

        // act
        var response = await Client.GetAsync(GetUrl(accountId));
        var payload = await response.ReadContentAsAsync<List<ApprenticeshipPaymentSummaryResponse>>();

        // assert
        response.Is.Ok();
        payload.Select(x => x.ApprenticeshipId).Should().BeEquivalentTo([first.ApprenticeshipId, second.ApprenticeshipId]);
    }

    [Test]
    public async Task Then_Only_The_Summaries_Awaiting_A_Payment_For_The_Period_Are_Returned()
    {
        // arrange
        var now = DateTime.UtcNow.ToPeriod();
        var accountId = AccountIdGenerator.Next();
        var awaitingPayment = await DbData.AddAsync(ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x
            .Started(monthsAgo: 3).TotalPaid(100).LastPayment(100, now.AddMonths(-1))));
        var zeroRecorded = await DbData.AddAsync(ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x
            .Started(monthsAgo: 3).TotalPaid(100).LastPayment(0, now)));
        var notStarted = await DbData.AddAsync(ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x.StartsIn(2)));
        await DbData.AddAsync(ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x
            .Started(monthsAgo: 3).TotalPaid(100).LastPayment(100, now)));
        await DbData.AddAsync(ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x
            .Started(monthsAgo: 3).TotalPaid(TestData.DefaultTotalCost)));
        await DbData.AddAsync(ApprenticeshipPaymentSummaryEntity.Create(accountId, x => x
            .Started(monthsAgo: 12).Length(6).TotalPaid(100)));

        // act
        var response = await Client.GetAsync(GetUrl(accountId, now));
        var payload = await response.ReadContentAsAsync<List<ApprenticeshipPaymentSummaryResponse>>();

        // assert
        response.Is.Ok();
        payload.Select(x => x.ApprenticeshipId).Should().BeEquivalentTo(
            [awaitingPayment.ApprenticeshipId, zeroRecorded.ApprenticeshipId, notStarted.ApprenticeshipId]);
    }
}
