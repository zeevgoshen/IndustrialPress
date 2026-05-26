using IndustrialPress.RestApi.Controllers;
using IndustrialPress.RestApi.Dtos;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace IndustrialPress.RestApi.UnitTests;

public class HealthControllerTests
{
    [Fact]
    public void Get_returns_healthy()
    {
        var controller = new HealthController();
        var result = controller.Get();
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<HealthResponseDto>(ok.Value);
        Assert.Equal("healthy", dto.Status);
        Assert.Equal("rest-api", dto.Service);
    }
}
