using System.Text.Json;
using System.Text.Json.Serialization;
using SFA.DAS.FundingProjection.Api.Models.Requests;
using SFA.DAS.FundingProjection.Api.Projection;
using SFA.DAS.FundingProjection.Domain.Enums;

namespace SFA.DAS.FundingProjection.Api.UnitTests.Api.Projection;

public class WhenMappingCurrentPeriodPayments
{
    [Test]
    public void Then_There_Are_No_Current_Payments_When_None_Are_Provided()
    {
        CurrentPeriodPayments.From(null).Should().BeNull();
    }

    [Test]
    public void Then_There_Are_No_Current_Payments_When_The_Period_End_Has_Not_Completed()
    {
        var payments = new CollectionPeriodPayments
        {
            PeriodEndCompleted = false,
            Payments = [Payment(FundingSource.Levy, TransactionType.Learning, 100m)]
        };

        CurrentPeriodPayments.From(payments).Should().BeNull();
    }

    [Test]
    public void Then_The_Current_Payments_Are_Zero_When_The_Period_End_Completed_With_No_Payments()
    {
        var result = CurrentPeriodPayments.From(new CollectionPeriodPayments { PeriodEndCompleted = true });

        result.Should().BeEquivalentTo(new CurrentPeriodPayments());
    }

    [Test]
    public void Then_The_Payments_Are_Split_By_Funding_Source_And_Completion()
    {
        var payments = new CollectionPeriodPayments
        {
            PeriodEndCompleted = true,
            Payments =
            [
                Payment(FundingSource.Levy, TransactionType.Learning, 100m),
                Payment(FundingSource.Levy, TransactionType.Balancing, 20m),
                Payment(FundingSource.Levy, TransactionType.Milestone1, 30m),
                Payment(FundingSource.Levy, TransactionType.Completion, 300m),
                Payment(FundingSource.LevyTransfer, TransactionType.Learning, 200m),
                Payment(FundingSource.LevyTransfer, TransactionType.Completion, 400m),
                Payment(FundingSource.CoInvestedEmployer, TransactionType.Learning, 5m),
                Payment(FundingSource.CoInvestedEmployer, TransactionType.Completion, 15m),
                Payment(FundingSource.CoInvestedSfa, TransactionType.Learning, 45m),
                Payment(FundingSource.CoInvestedSfa, TransactionType.Completion, 135m),
            ]
        };

        var result = CurrentPeriodPayments.From(payments);

        result.Should().BeEquivalentTo(new CurrentPeriodPayments
        {
            LearnerCost = 150m,
            LearnerFinalPaymentCost = 300m,
            TransferOut = 200m,
            TransferOutFinalPaymentCost = 400m,
            CoInvestment = 20m,
            DfeCoInvestment = 180m
        });
    }

    [Test]
    public void Then_Funding_Sources_Not_In_The_Enum_Are_Read_And_Ignored()
    {
        // arrange
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };
        const string json = """
            {
              "periodEndCompleted": true,
              "payments": [
                { "fundingSource": 1, "transactionType": 1, "amount": 100 },
                { "fundingSource": 4, "transactionType": 1, "amount": 25 },
                { "fundingSource": 99, "transactionType": 1, "amount": 10 }
              ]
            }
            """;

        // act
        var payments = JsonSerializer.Deserialize<CollectionPeriodPayments>(json, options);
        var result = CurrentPeriodPayments.From(payments);

        // assert
        payments!.Payments.Should().HaveCount(3);
        result.Should().BeEquivalentTo(new CurrentPeriodPayments { LearnerCost = 100m });
    }

    private static CollectionPeriodPaymentTotal Payment(FundingSource fundingSource, TransactionType transactionType, decimal amount) =>
        new() { FundingSource = fundingSource, TransactionType = transactionType, Amount = amount };
}
