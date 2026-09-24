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
        response.Month.Should().Be(entity.CalendarPeriodMonth);
        response.Year.Should().Be(entity.CalendarPeriodYear);
        response.CommittedLearnerCost.Should().Be(entity.CommittedLearnerCostTotal);
        response.CommittedTransferOut.Should().Be(entity.CommittedTransferOutTotal);
    }
}