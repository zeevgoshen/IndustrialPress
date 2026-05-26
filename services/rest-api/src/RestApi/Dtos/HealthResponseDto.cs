namespace IndustrialPress.RestApi.Dtos;

/// <summary>REST API health check.</summary>
public sealed class HealthResponseDto
{
    public string Service { get; set; } = "rest-api";
    public string Status { get; set; } = "healthy";
}
