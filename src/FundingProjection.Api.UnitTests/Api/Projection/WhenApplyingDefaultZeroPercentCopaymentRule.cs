using SFA.DAS.FundingProjection.Api.Projection;
using SFA.DAS.FundingProjection.Domain.Entities;

namespace SFA.DAS.FundingProjection.Api.UnitTests.Api.Projection;

public class WhenApplyingDefaultZeroPercentCopaymentRule
{
    [Test]
    public void Then_The_Rule_Is_Always_Applied()
    {
        // act
        var results = Enumerable.Range(2000, 100).Select(x => CopaymentRules.DefaultPercent0.CanRun(new ApprenticeshipPaymentSummaryEntity
        {
            StartDate = new DateOnly(x, 1, 1)
        } ));

        // assert
        results.Should().AllSatisfy(x => x.Should().BeTrue());
    }

    [Test]
    public void Then_The_Copayment_Amount_Is_Calculated_Correctly()
    {
        // act
        var actual = CopaymentRules.DefaultPercent0.CalculateCopayment(100m);

        // assert
        actual.Should().Be(0m);
    }
}