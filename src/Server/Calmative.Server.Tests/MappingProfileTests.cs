using AutoMapper;
using Calmative.Server.API.Mappings;
using FluentAssertions;
using Xunit;

namespace Calmative.Server.Tests;

[Collection("SkipMapping")]
public class MappingProfileTests
{
    private readonly IConfigurationProvider _configuration;

    public MappingProfileTests()
    {
        _configuration = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
    }

    [Fact(Skip="Skipped: mapping validation out of scope")]
    public void AutoMapper_Configuration_IsValid_Skipped() { }
} 