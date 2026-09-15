using FluentAssertions;
using SFA.DAS.FundingProjection.Api.Models.Mappers;
using SFA.DAS.FundingProjection.Domain.Entities;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.FundingProjection.Api.UnitTests.Api.Mappers;

[TestFixture]
internal class WhenMappingEmployerFundingProjection
{
    [Test, RecursiveMoqAutoData]
    public void ToGetResponse_ReturnsCorrectResponse(EmployerFundingProjectionEntity entity)
    {
        // Act
        var response = entity.ToGetResponse();

        // Assert
        response.EmployerAccountId.Should().Be(entity.EmployerAccountId);
        response.CommittedLearnerCostTotal.Should().Be(entity.CommittedLearnerCostTotal);
        response.CommittedTransferOutTotal.Should().Be(entity.CommittedTransferOutTotal);
        response.LastRecalculatedDate.Should().Be(entity.LastRecalculatedDate);
        response.CreatedDate.Should().Be(entity.CreatedDate);
    }
}