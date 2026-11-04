using SFA.DAS.FundingProjection.Api.Projection;
using SFA.DAS.FundingProjection.Domain.Entities;

namespace SFA.DAS.FundingProjection.Api.UnitTests.Api.Projection;

public class WhenApplying10PercentCopaymentRule
{
    public static readonly object[][] TestCases = [
        [new ApprenticeshipPaymentSummaryEntity { StartDate = new DateOnly(2019, 4, 1) }, false],
        [new ApprenticeshipPaymentSummaryEntity { StartDate = new DateOnly(2019, 3, 31) }, true],
        [new ApprenticeshipPaymentSummaryEntity { StartDate = new DateOnly(2017, 5, 1) }, true],
        [new ApprenticeshipPaymentSummaryEntity { StartDate = new DateOnly(2017, 4, 30) }, false],
    ];
    
    [TestCaseSource(nameof(TestCases))]
    public void Then_The_Rule_Is_Applied_Correctly(ApprenticeshipPaymentSummaryEntity entity, bool canRun)
    {
        // act
        var actual = CopaymentRules.Percent10.CanRun(entity);

        // assert
        actual.Should().Be(canRun);
    }

    [Test]
    public void Then_The_Copayment_Amount_Is_Calculated_Correctly()
    {
        // act
        var actual = CopaymentRules.Percent10.CalculateCopayment(200m);

        // assert
        actual.Should().Be(20m);
    }
}