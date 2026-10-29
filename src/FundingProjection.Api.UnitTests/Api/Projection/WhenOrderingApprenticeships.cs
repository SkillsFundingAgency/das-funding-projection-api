using SFA.DAS.FundingProjection.Api.Projection;
using SFA.DAS.FundingProjection.Domain.Entities;
using SFA.DAS.FundingProjection.Domain.Enums;

namespace SFA.DAS.FundingProjection.Api.UnitTests.Api.Projection;

public class WhenOrderingApprenticeships
{
    [Test, MoqAutoData]
    public void Then_Transferred_Apprenticeships_Are_Prioritised(ApprenticeshipOrderingStrategy sut)
    {
        // arrange
        var apprenticeships = new List<ApprenticeshipPaymentSummaryEntity>
        {
            new() { IsTransfer = false, },
            new() { IsTransfer = true, },
            new() { IsTransfer = false, },
        };

        // act
        var result = sut.Execute(apprenticeships).ToList();

        // assert
        result[0].IsTransfer.Should().BeTrue();
        result[1..].Should().AllSatisfy(x => x.IsTransfer.Should().BeFalse());
    }
    
    [Test, MoqAutoData]
    public void Then_Apprenticeships_Are_Prioritised_Over_Units(ApprenticeshipOrderingStrategy sut)
    {
        // arrange
        var apprenticeships = new List<ApprenticeshipPaymentSummaryEntity>
        {
            new() { LearningType = LearningType.ApprenticeshipUnit, },
            new() { LearningType = LearningType.Apprenticeship, },
            new() { LearningType = LearningType.ApprenticeshipUnit, },
        };

        // act
        var result = sut.Execute(apprenticeships).ToList();

        // assert
        result[0].LearningType.Should().Be(LearningType.Apprenticeship);
        result[1..].Should().AllSatisfy(x => x.LearningType.Should().Be(LearningType.ApprenticeshipUnit));
    }
    
    [Test, MoqAutoData]
    public void Then_ProviderOrder_Is_Ascending_Order(ApprenticeshipOrderingStrategy sut)
    {
        // arrange
        var apprenticeships = new List<ApprenticeshipPaymentSummaryEntity>
        {
            new() { ProviderId = 1, ProviderOrder = 2, },
            new() { ProviderId = 2, ProviderOrder = null, },
            new() { ProviderId = 3, ProviderOrder = 1, },
        };

        // act
        var result = sut.Execute(apprenticeships).ToList();

        // assert
        result[0].ProviderOrder.Should().Be(1);
        result[1].ProviderOrder.Should().Be(2);
        result[2].ProviderOrder.Should().Be(null);
    }
    
    [Test, MoqAutoData]
    public void Then_Null_ProviderOrder_Orders_Providers_By_Earliest_ApprovalDate(ApprenticeshipOrderingStrategy sut)
    {
        // arrange
        var apprenticeships = new List<ApprenticeshipPaymentSummaryEntity>
        {
            new() { ProviderId = 1, ApprovalDate = DateTime.UtcNow.AddDays(-26), },
            new() { ProviderId = 2, ApprovalDate = DateTime.UtcNow.AddDays(-45), },
            new() { ProviderId = 3, ApprovalDate = DateTime.UtcNow.AddDays(-100), },
            new() { ProviderId = 2, ApprovalDate = DateTime.UtcNow.AddDays(-15), },
            new() { ProviderId = 1, ApprovalDate = DateTime.UtcNow.AddDays(-10), },
            new() { ProviderId = 3, ApprovalDate = DateTime.UtcNow.AddDays(-76), },
        };

        // act
        var result = sut.Execute(apprenticeships).ToList();

        // assert
        result[0].ProviderId.Should().Be(3);
        result[1].ProviderId.Should().Be(3);
        result[2].ProviderId.Should().Be(2);
        result[3].ProviderId.Should().Be(2);
        result[4].ProviderId.Should().Be(1);
        result[5].ProviderId.Should().Be(1);
    }
    
    [Test, MoqAutoData]
    public void Then_ApprovalDate_Is_Ascending_Order(ApprenticeshipOrderingStrategy sut)
    {
        // arrange
        var apprenticeships = new List<ApprenticeshipPaymentSummaryEntity>
        {
            new() { ProviderId = 1, Uln = 3, ApprovalDate = DateTime.UtcNow.AddMonths(-10), },
            new() { ProviderId = 1, Uln = 2, ApprovalDate = DateTime.UtcNow.AddMonths(-50), },
            new() { ProviderId = 1, Uln = 1, ApprovalDate = DateTime.UtcNow.AddMonths(-100), },
        };

        // act
        var result = sut.Execute(apprenticeships).ToList();

        // assert
        result[0].Uln.Should().Be(1);
        result[1].Uln.Should().Be(2);
        result[2].Uln.Should().Be(3);
    }
    
    [Test, MoqAutoData]
    public void Then_Uln_Is_Ascending_Order(ApprenticeshipOrderingStrategy sut)
    {
        // arrange
        var apprenticeships = new List<ApprenticeshipPaymentSummaryEntity>
        {
            new() { ProviderId = 1, ProviderOrder = null, Uln = 23, },
            new() { ProviderId = 1, ProviderOrder = null, Uln = 1, },
            new() { ProviderId = 1, ProviderOrder = null, Uln = 52, },
        };

        // act
        var result = sut.Execute(apprenticeships).ToList();

        // assert
        result[0].Uln.Should().Be(1);
        result[1].Uln.Should().Be(23);
        result[2].Uln.Should().Be(52);
    }
    
    [Test, MoqAutoData]
    public void Then_LearningType_Is_Ordered_After_IsTransfer(ApprenticeshipOrderingStrategy sut)
    {
        // arrange
        var now = DateTime.UtcNow;
        var apprenticeships = new List<ApprenticeshipPaymentSummaryEntity>
        {
            new() { ProviderId = 1, IsTransfer = false, LearningType = LearningType.ApprenticeshipUnit, ProviderOrder = null, ApprovalDate = now.AddDays(-1), Uln = 1, },
            new() { ProviderId = 2, IsTransfer = true, LearningType = LearningType.ApprenticeshipUnit, ProviderOrder = null, ApprovalDate = now.AddDays(-1), Uln = 2, },
            new() { ProviderId = 3, IsTransfer = false, LearningType = LearningType.Apprenticeship, ProviderOrder = null, ApprovalDate = now.AddDays(-1), Uln = 3, },
            new() { ProviderId = 4, IsTransfer = true, LearningType = LearningType.Apprenticeship, ProviderOrder = null, ApprovalDate = now.AddDays(-1), Uln = 4, },
        };

        // act
        var result = sut.Execute(apprenticeships).ToList();

        // assert
        result[0].Uln.Should().Be(4);
        result[1].Uln.Should().Be(2);
        result[2].Uln.Should().Be(3);
        result[3].Uln.Should().Be(1);
    }
    
    [Test, MoqAutoData]
    public void Then_PriorityOrder_Is_Ordered_After_LearningType(ApprenticeshipOrderingStrategy sut)
    {
        // arrange
        var now = DateTime.UtcNow;
        var apprenticeships = new List<ApprenticeshipPaymentSummaryEntity>
        {
            new() { ProviderId = 1, IsTransfer = false, LearningType = LearningType.Apprenticeship, ProviderOrder = null, ApprovalDate = now.AddDays(-1), Uln = 1, },
            new() { ProviderId = 2, IsTransfer = false, LearningType = LearningType.Apprenticeship, ProviderOrder = 3, ApprovalDate = now.AddDays(-1), Uln = 2, },
            new() { ProviderId = 3, IsTransfer = false, LearningType = LearningType.Apprenticeship, ProviderOrder = 2, ApprovalDate = now.AddDays(-1), Uln = 3, },
            new() { ProviderId = 4, IsTransfer = false, LearningType = LearningType.Apprenticeship, ProviderOrder = 1, ApprovalDate = now.AddDays(-1), Uln = 4, },
        };

        // act
        var result = sut.Execute(apprenticeships).ToList();

        // assert
        result[0].Uln.Should().Be(4);
        result[1].Uln.Should().Be(3);
        result[2].Uln.Should().Be(2);
        result[3].Uln.Should().Be(1);
    }
    
    [Test, MoqAutoData]
    public void Then_Provider_First_Approval_Is_Ordered_After_ProviderOrder(ApprenticeshipOrderingStrategy sut)
    {
        // arrange
        var now = DateTime.UtcNow;
        var apprenticeships = new List<ApprenticeshipPaymentSummaryEntity>
        {
            new() { ProviderId = 1, IsTransfer = false, LearningType = LearningType.Apprenticeship, ProviderOrder = null, ApprovalDate = now.AddDays(-3), Uln = 1, },
            new() { ProviderId = 2, IsTransfer = false, LearningType = LearningType.Apprenticeship, ProviderOrder = null, ApprovalDate = now.AddDays(-2), Uln = 2, },
            new() { ProviderId = 3, IsTransfer = false, LearningType = LearningType.Apprenticeship, ProviderOrder = null, ApprovalDate = now.AddDays(-5), Uln = 3, },
            new() { ProviderId = 4, IsTransfer = false, LearningType = LearningType.Apprenticeship, ProviderOrder = null, ApprovalDate = now.AddDays(-9), Uln = 4, },
        };

        // act
        var result = sut.Execute(apprenticeships).ToList();

        // assert
        result[0].Uln.Should().Be(4);
        result[1].Uln.Should().Be(3);
        result[2].Uln.Should().Be(1);
        result[3].Uln.Should().Be(2);
    }
    
    [Test, MoqAutoData]
    public void Then_ApprovalDate_Is_Ordered_After_Provider_First_Approval(ApprenticeshipOrderingStrategy sut)
    {
        // arrange
        var now = DateTime.UtcNow;
        var apprenticeships = new List<ApprenticeshipPaymentSummaryEntity>
        {
            new() { ProviderId = 1, IsTransfer = false, LearningType = LearningType.Apprenticeship, ProviderOrder = 1, ApprovalDate = now.AddDays(-3), Uln = 1, },
            new() { ProviderId = 2, IsTransfer = false, LearningType = LearningType.Apprenticeship, ProviderOrder = 1, ApprovalDate = now.AddDays(-2), Uln = 2, },
            new() { ProviderId = 3, IsTransfer = false, LearningType = LearningType.Apprenticeship, ProviderOrder = 1, ApprovalDate = now.AddDays(-5), Uln = 3, },
            new() { ProviderId = 4, IsTransfer = false, LearningType = LearningType.Apprenticeship, ProviderOrder = 1, ApprovalDate = now.AddDays(-9), Uln = 4, },
        };

        // act
        var result = sut.Execute(apprenticeships).ToList();

        // assert
        result[0].Uln.Should().Be(4);
        result[1].Uln.Should().Be(3);
        result[2].Uln.Should().Be(1);
        result[3].Uln.Should().Be(2);
    }
    
    [Test, MoqAutoData]
    public void Then_Uln_Is_Ordered_After_ApprovalDate(ApprenticeshipOrderingStrategy sut)
    {
        // arrange
        var now = DateTime.UtcNow;
        var apprenticeships = new List<ApprenticeshipPaymentSummaryEntity>
        {
            new() { ProviderId = 1, IsTransfer = false, LearningType = LearningType.Apprenticeship, ProviderOrder = 1, ApprovalDate = now.AddMonths(-1), Uln = 5, },
            new() { ProviderId = 2, IsTransfer = false, LearningType = LearningType.Apprenticeship, ProviderOrder = 1, ApprovalDate = now.AddMonths(-1), Uln = 2, },
            new() { ProviderId = 4, IsTransfer = false, LearningType = LearningType.Apprenticeship, ProviderOrder = 1, ApprovalDate = now.AddMonths(-2), Uln = 9, },
            new() { ProviderId = 3, IsTransfer = false, LearningType = LearningType.Apprenticeship, ProviderOrder = 1, ApprovalDate = now.AddMonths(-2), Uln = 1, },
        };

        // act
        var result = sut.Execute(apprenticeships).ToList();

        // assert
        result[0].Uln.Should().Be(1);
        result[1].Uln.Should().Be(9);
        result[2].Uln.Should().Be(2);
        result[3].Uln.Should().Be(5);
    }
}