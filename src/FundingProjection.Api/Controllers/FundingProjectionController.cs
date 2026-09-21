using Microsoft.AspNetCore.Mvc;
using SFA.DAS.FundingProjection.Data.Repositories;
using System.ComponentModel.DataAnnotations;
using System.Net;
using SFA.DAS.FundingProjection.Api.Core;
using SFA.DAS.FundingProjection.Api.Models.Mappers;
using SFA.DAS.FundingProjection.Api.Models.Responses;
using SFA.DAS.FundingProjection.Data.Services;

namespace SFA.DAS.FundingProjection.Api.Controllers;

[ApiController]
public class FundingProjectionController(
    [FromServices] IEmployerFundingProjectionRepository repository,
    ILogger<FundingProjectionController> logger) : ControllerBase
{
    [HttpGet]
    [Route($"{RouteNames.EmployerFundingProjection}/{{accountId:long}}/{RouteElements.FundingProjection}")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(GetEmployerFundingProjectionResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetEmployerFundingProjection(
        [FromRoute] [Required] long accountId,
        CancellationToken token)
    {
        try
        {
            logger.LogInformation("Funding Projection API: Received query to get projection by accountId : {Id}", accountId);

            var response = await repository.GetByEmployerAccountIdAsync(accountId, token);

            return response == null
                ? Results.NotFound()
                : TypedResults.Ok(response.ToGetResponse());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to Get funding projection : An error occurred");
            return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
        }
    }

    [HttpPost]
    [Route($"{RouteNames.EmployerFundingProjection}/{RouteElements.FundingProjection}/update")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IResult> UpdateEmployerFundingProjection(
        [FromQuery] DateTime cutOffDateTime,
        [FromServices] IFundingProjectionServices fundingProjectionServices,
        CancellationToken token)
    {
        try
        {
            logger.LogInformation("Funding Projection API: Received request to update projection");

            var result = await fundingProjectionServices.UpdateProjection(cutOffDateTime, token);

            return Results.Ok(result);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to Update funding projection : An error occurred");
            return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
        }
    }
}