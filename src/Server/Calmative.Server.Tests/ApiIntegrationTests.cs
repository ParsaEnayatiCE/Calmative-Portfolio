using System.Net;
using System.Net.Http.Json;
using Calmative.Server.API;
using Calmative.Server.API.DTOs;
using Calmative.Server.API.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Calmative.Server.Tests;

public class ApiIntegrationTests
{
    private WebApplicationFactory<Program> CreateFactory()
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null) services.Remove(descriptor);
                    services.AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase("IntegrationDB"));
                });
            });
    }

    [Fact]
    public async Task HealthEndpoint_ShouldReturnOk()
    {
        var factory = CreateFactory();
        var client = factory.CreateClient();
        var res = await client.GetAsync("/health");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RegisterAndLogin_Flow_ShouldReturnJwt()
    {
        var factory = CreateFactory();
        var client = factory.CreateClient();

        var register = new RegisterDto { Email="int@test.com",Password="Pass123$",ConfirmPassword="Pass123$",FirstName="Int",LastName="Test"};
        var regRes = await client.PostAsJsonAsync("/api/auth/register",register);
        regRes.EnsureSuccessStatusCode();

        using(var scope = factory.Services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await ctx.Users.FirstAsync(u=>u.Email=="int@test.com");
            user.IsEmailConfirmed=true;
            await ctx.SaveChangesAsync();
        }

        var login = new LoginDto{Email="int@test.com",Password="Pass123$"};
        var logRes = await client.PostAsJsonAsync("/api/auth/login",login);
        logRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var authDto = await logRes.Content.ReadFromJsonAsync<AuthResponseDto>();
        authDto!.Token.Should().NotBeNullOrEmpty();
    }
} 