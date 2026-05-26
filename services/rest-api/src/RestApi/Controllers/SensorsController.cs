using IndustrialPress.RestApi.Dtos;
using IndustrialPress.RestApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace IndustrialPress.RestApi.Controllers;

/// <summary>
/// Sensor metadata (SQL). Live telemetry is pushed via SignalR at /hubs/telemetry — not available on REST.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class SensorsController(SensorMetadataGateway sql) : ControllerBase
{
    /// <summary>List all 20 sensors (metadata).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SensorMetadataDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<IEnumerable<SensorMetadataDto>>> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var sensors = await sql.GetSensorsAsync(cancellationToken);
            return Ok(sensors.Select(s => new SensorMetadataDto
            {
                Id = s.Id,
                Name = s.Name,
                Location = s.Location,
                Type = s.Type,
                Enabled = s.Enabled
            }));
        }
        catch
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new ErrorResponseDto { Error = "SQL Data service unavailable" });
        }
    }

    /// <summary>Get one sensor by id (metadata).</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SensorMetadataDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<SensorMetadataDto>> GetById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var sensor = await sql.GetSensorAsync(id, cancellationToken);
            if (sensor is null)
                return NotFound();

            return Ok(new SensorMetadataDto
            {
                Id = sensor.Id,
                Name = sensor.Name,
                Location = sensor.Location,
                Type = sensor.Type,
                Enabled = sensor.Enabled
            });
        }
        catch
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new ErrorResponseDto { Error = "SQL Data service unavailable" });
        }
    }
}
