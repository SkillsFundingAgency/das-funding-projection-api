using FundingProjection.Api.Testing;
using FundingProjection.Api.Testing.DataCreation;
using SFA.DAS.FundingProjection.Api.Models.Requests;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Domain.Entities;

namespace FundingProjection.Api.IntegrationTests.Controllers.ApprenticeshipPaymentSummaryControllerTests;

public class WhenPuttingAnApprenticeshipPaymentSummary: SqlBaseFixture
{
    private static Uri PutUrl(long accountId, long apprenticeshipId) => new($"api/employer/{accountId}/apprenticeship-payment-summary/{apprenticeshipId}", UriKind.Relative);
    
    [Test, MoqAutoData]
    public async Task Then_The_Record_Is_Created(PostApprenticeshipPaymentSummaryRequest request)
    {
        // arrange
        var accountId = AccountIdGenerator.Next();
        var apprenticeshipId = ApprenticeshipIdGenerator.Next();
        request.Uln = UlnGenerator.Next();
        
        // act
        var response = await Client.PutAsJsonAsync(PutUrl(accountId, apprenticeshipId), request);
        var payload = await response.ReadContentAsAsync<ApprenticeshipPaymentSummaryResponse>();
        var entity = await DbData.GetAsync<ApprenticeshipPaymentSummaryEntity>(apprenticeshipId);

        // assert
        response.Is.Created();
        entity.Should().NotBeNull();
        entity.Should().BeEquivalentTo(request);
        entity.AccountId.Should().Be(accountId);
        entity.ApprenticeshipId.Should().Be(apprenticeshipId);
        payload.Should().BeEquivalentTo(entity);
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Record_Is_Upserted(PostApprenticeshipPaymentSummaryRequest request)
    {
        // arrange
        var accountId = AccountIdGenerator.Next();
        var apprenticeshipId = ApprenticeshipIdGenerator.Next();
        request.Uln = UlnGenerator.Next();
        await Client.PutAsJsonAsync(PutUrl(accountId, apprenticeshipId), request);
        request.TotalCost *= 2;
        request.ProviderOrder += 10;
        
        // act
        var response = await Client.PutAsJsonAsync(PutUrl(accountId, apprenticeshipId), request);
        var payload = await response.ReadContentAsAsync<ApprenticeshipPaymentSummaryResponse>();
        var entity = await DbData.GetAsync<ApprenticeshipPaymentSummaryEntity>(apprenticeshipId);

        // assert
        response.Is.Ok();
        entity.Should().NotBeNull();
        entity.Should().BeEquivalentTo(request);
        entity.AccountId.Should().Be(accountId);
        entity.ApprenticeshipId.Should().Be(apprenticeshipId);
        payload.Should().BeEquivalentTo(entity);
    }
}