using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using SFA.DAS.FundingProjection.Api.Controllers;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Api.Projection;
using SFA.DAS.FundingProjection.Data.Repositories;
using SFA.DAS.FundingProjection.Domain.Entities;
using SFA.DAS.FundingProjection.Domain.Enums;

namespace SFA.DAS.FundingProjection.Api.UnitTests.Api.Controller.EmployerFundingProjectionControllerTests;

[TestFixture]
internal class WhenGettingProjectionMonthsByAccountId
{
    private const long AccountId = 1;

    [TestCase(17, 19)]
    [TestCase(10, 12)]
    [TestCase(118, 120)]
    public async Task Then_The_Months_Run_From_This_Month_To_The_Month_After_The_Latest_End_Date(int endMonthsAhead, int expectedMonths)
    {
        // arrange
        var now = StartOfThisMonth();

        // act
        var months = await GetProjectionMonths(
            CreateSummary(1, ApprenticeshipStatus.Live, now.AddMonths(3), isTransfer: false),
            CreateSummary(2, ApprenticeshipStatus.WaitingToStart, now.AddMonths(endMonthsAhead).AddDays(14), isTransfer: true),
            CreateSummary(3, ApprenticeshipStatus.Completed, now.AddMonths(-6), isTransfer: false));

        // assert
        months.Should().Be(expectedMonths);
    }

    [TestCase(ApprenticeshipStatus.Paused)]
    [TestCase(ApprenticeshipStatus.Stopped)]
    [TestCase(ApprenticeshipStatus.Unknown)]
    public async Task Then_Apprenticeships_That_Are_Not_Projected_Are_Ignored(ApprenticeshipStatus status)
    {
        // arrange
        var now = StartOfThisMonth();

        // act
        var months = await GetProjectionMonths(
            CreateSummary(1, ApprenticeshipStatus.Live, now.AddMonths(17), isTransfer: false),
            CreateSummary(2, status, now.AddMonths(40), isTransfer: false));

        // assert
        months.Should().Be(19);
    }

    [TestCase(0)]
    [TestCase(5)]
    [TestCase(-3)]
    public async Task Then_At_Least_12_Months_Are_Projected(int endMonthsAhead)
    {
        // act
        var months = await GetProjectionMonths(
            CreateSummary(1, ApprenticeshipStatus.Live, StartOfThisMonth().AddMonths(endMonthsAhead), isTransfer: false));

        // assert
        months.Should().Be(12);
    }

    [Test]
    public async Task Then_12_Months_Are_Projected_When_There_Are_No_Apprenticeships()
    {
        // act
        var months = await GetProjectionMonths();

        // assert
        months.Should().Be(12);
    }

    [Test]
    public async Task Then_No_More_Than_120_Months_Are_Projected()
    {
        // act
        var months = await GetProjectionMonths(
            CreateSummary(1, ApprenticeshipStatus.Live, StartOfThisMonth().AddYears(20), isTransfer: false));

        // assert
        months.Should().Be(120);
    }

    private static async Task<int> GetProjectionMonths(params ApprenticeshipPaymentSummaryEntity[] summaries)
    {
        var apprenticeshipSummariesRepository = new Mock<IApprenticeshipPaymentSummariesRepository>();
        apprenticeshipSummariesRepository
            .Setup(x => x.GetApprenticeshipPaymentSummariesAsync(AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([.. summaries]);
        var controller = new FundingProjectionController(Mock.Of<ILogger<FundingProjectionController>>());

        var result = await controller.GetProjectionMonths(apprenticeshipSummariesRepository.Object, AccountId) as Ok<ProjectionMonthsResponse>;

        result.Should().NotBeNull();
        return result!.Value!.Months;
    }

    private static DateOnly StartOfThisMonth() => DateOnly.FromDateTime(DateTime.UtcNow).ToPeriod();

    private static ApprenticeshipPaymentSummaryEntity CreateSummary(long apprenticeshipId, ApprenticeshipStatus status, DateOnly endDate, bool isTransfer) => new()
    {
        ApprenticeshipId = apprenticeshipId,
        Status = status,
        StartDate = endDate.AddYears(-1),
        EndDate = endDate,
        IsTransfer = isTransfer,
        TotalCost = 1000m,
    };
}
