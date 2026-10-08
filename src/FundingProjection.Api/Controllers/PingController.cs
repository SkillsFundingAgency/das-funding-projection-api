using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.FundingProjection.Api.Controllers;

[ApiController]
[Route("ping")]
public class PingController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("pong");
    }
}