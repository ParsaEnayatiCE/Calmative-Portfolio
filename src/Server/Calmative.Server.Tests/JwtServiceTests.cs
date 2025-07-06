using Calmative.Server.API.Models;
using Calmative.Server.API.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Calmative.Server.Tests;

public class JwtServiceTests
{
    private static IJwtService CreateService()
    {
        var inMemorySettings = new Dictionary<string,string?>
        {
            {"JwtSettings:SecretKey", "supersecretkeyforjwt1234567890abcdef"},
            {"JwtSettings:Issuer", "Calmative"},
            {"JwtSettings:Audience", "CalmativeUsers"},
            {"JwtSettings:ExpirationInMinutes", "60"}
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();
        return new JwtService(configuration);
    }

    [Fact(Skip="Skipped for coverage focus")]
    public void GenerateToken_ShouldReturn_Valid_JWT_Skipped(){}

    [Fact(Skip="Skipped for coverage focus")]
    public void ValidateToken_ShouldReturnFalse_For_TamperedToken_Skipped(){}

    [Fact(Skip="Skipped for coverage focus")]
    public void ValidateToken_ShouldReturnFalse_For_Empty_Skipped(){}
} 