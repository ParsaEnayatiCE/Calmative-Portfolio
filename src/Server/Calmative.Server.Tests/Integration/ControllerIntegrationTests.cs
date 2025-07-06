using System.Net;
using System.Net.Http.Json;
using Calmative.Server.API.Models;
using Calmative.Server.API.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;

namespace Calmative.Server.Tests.Integration;

public class ControllerIntegrationTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;

    public ControllerIntegrationTests(TestApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AssetTypes_Should_Return_Ok_With_TestAuth()
    {
        var client = _factory.CreateClient();
        var res = await client.GetAsync("/api/Asset/types");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AssetTypes_Should_Return_Unauthorized_Without_Auth()
    {
        using var rawFactory = new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>();
        var client = rawFactory.CreateClient();
        var res = await client.GetAsync("/api/Asset/types");
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
} 