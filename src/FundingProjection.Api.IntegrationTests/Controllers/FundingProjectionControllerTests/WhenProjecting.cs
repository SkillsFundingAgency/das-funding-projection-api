using FundingProjection.Api.Testing;
using FundingProjection.Api.Testing.DataCreation;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Api.Projection;
using SFA.DAS.FundingProjection.Domain.Entities;

namespace FundingProjection.Api.IntegrationTests.Controllers.FundingProjectionControllerTests;

public class WhenProjecting: SqlBaseFixture
{
    /*
     One big smoke test - seemed a good idea the time
     */
    [Test]
    public async Task Then_The_Projection_Should_Be_Correct()
    {
        // arrange
        var accountId = AccountIdGenerator.Next();
        await DbData.AddManyAsync([
            // not transferred
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => // 1. completed long ago - should not be included
                x.Started(24).TotalCost(10000).TotalPaid(10000)),
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => // 2. completed recently - final payment due in current period
                x.Started(8).TotalPaid(700m).LastPayment(100m, DateTime.UtcNow.AddMonths(-1).ToPeriod()).Live()),
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => // 3. completed recently - final payment completed in current period
                x.Started(8).TotalPaid(1000m).LastPayment(300m, DateTime.UtcNow.ToPeriod())),
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => // 4. live - payments due in current period
                x.Started(4).TotalPaid(300m).LastPayment(100m, DateTime.UtcNow.AddMonths(-1).ToPeriod())),
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => // 5. live - payments completed in current period
                x.Started(4).TotalPaid(400m).LastPayment(100m, DateTime.UtcNow.ToPeriod())),
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => // 6. waiting to start
                x.StartsIn(4)),
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => // 7. Stopped
                x.Started(4).Stopped()),
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => // 8. Paused
                x.Started(4).Paused()),
            
            // transferred
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => // 1. completed long ago - should not be included
                x.Started(24).TotalCost(10000).TotalPaid(10000).Transferred()),
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => // 2. completed recently - final payment due in current period
                x.Started(8).TotalPaid(700m).LastPayment(100m, DateTime.UtcNow.AddMonths(-1).ToPeriod()).Live().Transferred()),
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => // 3. completed recently - final payment completed in current period
                x.Started(8).TotalPaid(1000m).LastPayment(300m, DateTime.UtcNow.ToPeriod()).Transferred()),
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => // 4. live - payments due in current period
                x.Started(4).TotalPaid(300m).LastPayment(100m, DateTime.UtcNow.AddMonths(-1).ToPeriod()).Transferred()),
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => // 5. live - payments completed in current period
                x.Started(4).TotalPaid(400m).LastPayment(100m, DateTime.UtcNow.ToPeriod()).Transferred()),
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => // 6. waiting to start
                x.StartsIn(4).Transferred()),
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => // 7. Stopped
                x.Started(4).Transferred().Stopped()),
            ApprenticeshipPaymentSummaryEntity.Create(accountId, x => // 8. Paused
                x.Started(4).Transferred().Paused()),
        ]);
        
        /*
         Period 1
         Test Case  Monthly     Final   
         -------------------------------
         1/6-8      0           0       
         2          100         200     
         3          100         200     
         4          100         0       
         5          100         0       
         -------------------------------
                    400         400     
         -------------------------------
         
         Periods 2-4
         Test Case  Monthly     Final 
         -----------------------------
         1-3+6-8    0           0     
         4          100         0     
         5          100         0     
         -----------------------------
                    200         0
         -----------------------------
         
         Period 5
         Test Case  Monthly     Final 
         -----------------------------
         1-3+6-8    0           0     
         4          100         200     
         5          100         200     
         -----------------------------
                    200         400
         -----------------------------
         
         Period 6
         Test Case  Monthly     Final 
         -----------------------------
         1-5+7-8    0           0     
         6          100         0
         -----------------------------
                    100         0
         -----------------------------
         */

        // act
        var response = await Measure.ThisAsync(async () => await Client.PostAsJsonAsync(ProjectionUrl.Create(accountId), ProjectionRequest.Create(6, 120001m, 20000m)));
        var timeline = await response.ReadContentAsAsync<EstimatesTimeline>();

        // assert
        response.Is.Ok();
        timeline.Should().NotBeNull();
        
        timeline.TotalEstimatedCommitedCost.Should().Be(3600m);
        timeline.TotalEstimatedCommitedTransferCost.Should().Be(3600m);
        
        timeline.Projections.Should().HaveCount(6);
        
        timeline.Projections[0].LevyOutSummary.CommittedLearnerCost.Should().Be(400);
        timeline.Projections[0].LevyOutSummary.CommittedLearnerFinalPaymentCost.Should().Be(400);
        timeline.Projections[0].LevyOutSummary.CommittedTransferOut.Should().Be(400);
        timeline.Projections[0].LevyOutSummary.CommittedTransferOutFinalPaymentCost.Should().Be(400);
        
        timeline.Projections[1].LevyOutSummary.CommittedLearnerCost.Should().Be(200);
        timeline.Projections[1].LevyOutSummary.CommittedLearnerFinalPaymentCost.Should().Be(0);
        timeline.Projections[1].LevyOutSummary.CommittedTransferOut.Should().Be(200);
        timeline.Projections[1].LevyOutSummary.CommittedTransferOutFinalPaymentCost.Should().Be(0);
        
        timeline.Projections[2].LevyOutSummary.CommittedLearnerCost.Should().Be(200);
        timeline.Projections[2].LevyOutSummary.CommittedLearnerFinalPaymentCost.Should().Be(0);
        timeline.Projections[2].LevyOutSummary.CommittedTransferOut.Should().Be(200);
        timeline.Projections[2].LevyOutSummary.CommittedTransferOutFinalPaymentCost.Should().Be(0);
        
        timeline.Projections[3].LevyOutSummary.CommittedLearnerCost.Should().Be(200);
        timeline.Projections[3].LevyOutSummary.CommittedLearnerFinalPaymentCost.Should().Be(0);
        timeline.Projections[3].LevyOutSummary.CommittedTransferOut.Should().Be(200);
        timeline.Projections[3].LevyOutSummary.CommittedTransferOutFinalPaymentCost.Should().Be(0);
        
        timeline.Projections[4].LevyOutSummary.CommittedLearnerCost.Should().Be(200);
        timeline.Projections[4].LevyOutSummary.CommittedLearnerFinalPaymentCost.Should().Be(400);
        timeline.Projections[4].LevyOutSummary.CommittedTransferOut.Should().Be(200);
        timeline.Projections[4].LevyOutSummary.CommittedTransferOutFinalPaymentCost.Should().Be(400);
        
        timeline.Projections[5].LevyOutSummary.CommittedLearnerCost.Should().Be(100);
        timeline.Projections[5].LevyOutSummary.CommittedLearnerFinalPaymentCost.Should().Be(0);
        timeline.Projections[5].LevyOutSummary.CommittedTransferOut.Should().Be(100);
        timeline.Projections[5].LevyOutSummary.CommittedTransferOutFinalPaymentCost.Should().Be(0);
    }
}