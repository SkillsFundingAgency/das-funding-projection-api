using SFA.DAS.FundingProjection.Api.UnitTests.Data.DatabaseMock;
using SFA.DAS.FundingProjection.Data;
using SFA.DAS.FundingProjection.Data.Repositories;
using SFA.DAS.FundingProjection.Domain.Entities;

namespace SFA.DAS.FundingProjection.Api.UnitTests.Data.Repositories.EmployerFundingProjectionRepositoryTests;

[TestFixture]
internal class WhenGettingApprenticeshipSummariesAwaitingPaymentAsync
{
    private const long AccountId = 12345;
    private static readonly DateOnly PaymentPeriod = new(2025, 11, 1);

    [Test, RecursiveMoqAutoData]
    public async Task Then_Summaries_Still_Waiting_On_A_Payment_For_The_Period_Are_Returned(
        CancellationToken token,
        [Frozen] Mock<IFundingProjectionDataContext> context,
        [Greedy] ApprenticeshipPaymentSummariesRepository repository)
    {
        var awaitingPayment = CreateSummary(1);
        var zeroRecordedForThePeriod = CreateSummary(2, lastPaymentAmount: 0m, lastPaymentDate: PaymentPeriod);
        var notStarted = CreateSummary(3, startDate: new DateOnly(2026, 1, 1), lastPaymentAmount: null, lastPaymentDate: null);
        // Schedule ends October, November is the extra month for a late completion payment
        var endedLastMonth = CreateSummary(4, endDate: new DateOnly(2025, 9, 30));
        context.Setup(x => x.ApprenticeshipPaymentSummaries)
            .ReturnsDbSet([awaitingPayment, zeroRecordedForThePeriod, notStarted, endedLastMonth]);

        var actual = await repository.GetApprenticeshipPaymentSummariesAwaitingPaymentAsync(AccountId, PaymentPeriod, token);

        actual.Should().BeEquivalentTo([awaitingPayment, zeroRecordedForThePeriod, notStarted, endedLastMonth]);
    }

    [Test, RecursiveMoqAutoData]
    public async Task Then_Summaries_That_Do_Not_Need_Checking_Are_Not_Returned(
        CancellationToken token,
        [Frozen] Mock<IFundingProjectionDataContext> context,
        [Greedy] ApprenticeshipPaymentSummariesRepository repository)
    {
        var otherAccount = CreateSummary(1, accountId: AccountId + 1);
        var fullyPaid = CreateSummary(2, totalPaid: 1200m);
        var alreadyPaidForThePeriod = CreateSummary(3, lastPaymentAmount: 100m, lastPaymentDate: PaymentPeriod);
        var endedTooLongAgo = CreateSummary(4, endDate: new DateOnly(2025, 8, 31));
        context.Setup(x => x.ApprenticeshipPaymentSummaries)
            .ReturnsDbSet([otherAccount, fullyPaid, alreadyPaidForThePeriod, endedTooLongAgo]);

        var actual = await repository.GetApprenticeshipPaymentSummariesAwaitingPaymentAsync(AccountId, PaymentPeriod, token);

        actual.Should().BeEmpty();
    }

    private static ApprenticeshipPaymentSummaryEntity CreateSummary(
        long apprenticeshipId,
        long accountId = AccountId,
        decimal totalPaid = 100m,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        decimal? lastPaymentAmount = 100m,
        DateOnly? lastPaymentDate = null) => new()
    {
        ApprenticeshipId = apprenticeshipId,
        AccountId = accountId,
        TotalCost = 1200m,
        TotalPaid = totalPaid,
        StartDate = startDate ?? new DateOnly(2024, 1, 1),
        EndDate = endDate ?? new DateOnly(2026, 1, 1),
        LastPaymentAmount = lastPaymentAmount,
        LastPaymentDate = lastPaymentDate ?? (lastPaymentAmount is null ? null : new DateOnly(2025, 10, 1)),
    };
}
