using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using FluentAssertions;

namespace Calmative.Server.Tests;

public class ProgramSmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProgramSmokeTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Auth_Test_Endpoint_Returns_OK()
    {
        var res = await _client.GetAsync("/api/Auth/test");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }
} 