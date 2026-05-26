using IndustrialPress.RestApi.Controllers;
using IndustrialPress.RestApi.Dtos;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace IndustrialPress.RestApi.UnitTests;

public class SensorsControllerTests
{
    [Fact]
    public async Task GetAll_returns_twenty_sensors()
    {
        var controller = new SensorsController(new FakeSensorMetadataGateway());
        var result = await controller.GetAll(CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<IEnumerable<SensorMetadataDto>>(ok.Value).ToList();
        Assert.Equal(20, list.Count);
    }

    [Fact]
    public async Task GetById_returns_sensor_when_found()
    {
        var controller = new SensorsController(new FakeSensorMetadataGateway());
        var result = await controller.GetById(7, CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<SensorMetadataDto>(ok.Value);
        Assert.Equal(7, dto.Id);
    }

    [Fact]
    public async Task GetById_returns_not_found_for_missing()
    {
        var gateway = new FakeSensorMetadataGateway();
        var controller = new SensorsController(gateway);
        var result = await controller.GetById(99, CancellationToken.None);
        Assert.IsType<NotFoundResult>(result.Result);
    }
}
