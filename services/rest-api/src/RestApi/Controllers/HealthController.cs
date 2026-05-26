using IndustrialPress.RestApi.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace IndustrialPress.RestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class HealthController : ControllerBase
{
    /// <summary>Returns REST API health (for UI system page and ops).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(HealthResponseDto), StatusCodes.Status200OK)]
    public ActionResult<HealthResponseDto> Get() =>
        Ok(new HealthResponseDto { Service = "rest-api", Status = "healthy" });
}
