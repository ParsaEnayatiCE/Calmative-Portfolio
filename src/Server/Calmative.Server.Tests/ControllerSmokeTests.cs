using System.Net;
using System.Threading.Tasks;
using Calmative.Server.API.Controllers;
using Calmative.Server.API.Services;
using Calmative.Server.API.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Calmative.Server.Tests;

public class ControllerSmokeTests
{
    private static ILogger<T> DummyLogger<T>() => new Mock<ILogger<T>>().Object;

    // ----------------- AssetController -----------------
    [Fact]
    public async Task AssetController_GetLatestPrice_Returns_Ok()
    {
        var mock = new Mock<IAssetService>();
        mock.Setup(s => s.GetLatestPriceAsync("BTC")).ReturnsAsync(100m);
        var ctrl = new AssetController(mock.Object, DummyLogger<AssetController>());

        var res = await ctrl.GetLatestPrice("BTC");
        res.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(100m);
    }

    [Fact]
    public async Task AssetController_GetLatestPrice_Returns_NotFound()
    {
        var mock = new Mock<IAssetService>();
        mock.Setup(s => s.GetLatestPriceAsync("DOGE")).ReturnsAsync((decimal?)null);
        var ctrl = new AssetController(mock.Object, DummyLogger<AssetController>());

        var res = await ctrl.GetLatestPrice("DOGE");
        res.Should().BeOfType<NotFoundResult>();
    }

    // ----------------- PortfolioController -----------------
    [Fact]
    public async Task PortfolioController_Create_Returns_BadRequest_When_Service_Fails()
    {
        var mock = new Mock<IPortfolioService>();
        mock.Setup(s => s.CreatePortfolioAsync(It.IsAny<CreatePortfolioDto>(), 1)).ReturnsAsync((PortfolioDto?)null);
        var ctrl = new PortfolioController(mock.Object, DummyLogger<PortfolioController>());

        var res = await ctrl.Create(new CreatePortfolioDto(), 1);
        res.Should().BeOfType<BadRequestResult>();
    }

    // ----------------- AuthController -----------------
    [Fact]
    public async Task AuthController_Login_Unauthorized_On_Invalid_Creds()
    {
        var mock = new Mock<IAuthService>();
        mock.Setup(s => s.LoginAsync(It.IsAny<LoginDto>())).ReturnsAsync((false, null, "invalid"));
        var ctrl = new AuthController(mock.Object, new ConfigurationBuilder().Build());

        var res = await ctrl.Login(new LoginDto());
        res.Should().BeOfType<UnauthorizedObjectResult>();
    }

    // Additional controller tests can be added here as needed
} 