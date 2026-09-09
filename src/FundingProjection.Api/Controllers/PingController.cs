using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.FundingProjection.Api.Controllers;

[ApiController]
public class PingController : ControllerBase
{
    [HttpGet]
    [Route("ping")]
    public IActionResult Get()
    {
        return Ok("pong");
    }
}