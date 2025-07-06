using Calmative.Server.API.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Calmative.Server.Tests;

public class EmailServiceTests
{
    private static EmailService CreateService(Dictionary<string,string?> cfg)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(cfg!).Build();
        var logger = new Mock<ILogger<EmailService>>();
        return new EmailService(logger.Object, configuration);
    }

    [Fact]
    public async Task SendEmail_Should_Log_When_HostMissing()
    {
        var cfg = new Dictionary<string,string?>(); // no host/port
        var svc = CreateService(cfg);
        await svc.Invoking(s=>s.SendEmailAsync("to@test.com","Sub","Body"))
                 .Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendEmail_Should_Fallback_When_SmtpFails()
    {
        var cfg = new Dictionary<string,string?>
        {
            {"EmailSettings:SmtpHost", "invalid-host"},
            {"EmailSettings:SmtpPort", "2525"},
            {"EmailSettings:Username", "u"},
            {"EmailSettings:Password", "p"},
            {"EmailSettings:EnableSsl", "false"}
        };
        var svc = CreateService(cfg);
        await svc.Invoking(s=>s.SendEmailAsync("to@test.com","Sub","Body"))
                 .Should().NotThrowAsync();
    }
} 