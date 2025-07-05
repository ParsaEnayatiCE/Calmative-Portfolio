using AutoMapper;
using Calmative.Server.API.Data;
using Calmative.Server.API.DTOs;
using Calmative.Server.API.Models;
using Calmative.Server.API.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using BCrypt.Net;

namespace Calmative.Server.Tests;

public class AuthServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly Mock<IJwtService> _jwtMock = new();
    private readonly Mock<IEmailService> _emailMock = new();
    private readonly IConfiguration _config;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _context = TestHelper.GetInMemoryDbContext(Guid.NewGuid().ToString());
        _mapper = TestHelper.GetMapper();
        var dict = new Dictionary<string,string>{{"FrontendSettings:BaseUrl","http://localhost"}};
        _config = new ConfigurationBuilder().AddInMemoryCollection(dict!).Build();
        _jwtMock.Setup(j=>j.GenerateToken(It.IsAny<User>())).Returns("test-token");
        _service = new AuthService(_context,_mapper,_jwtMock.Object,_emailMock.Object,_config);
    }

    [Fact]
    public async Task Register_ShouldSucceedAndSendEmail()
    {
        var dto = new RegisterDto{Email="a@b.com",Password="Pa$$w0rd",FirstName="A",LastName="B"};
        var result = await _service.RegisterAsync(dto);
        result.Succeeded.Should().BeTrue();
        _emailMock.Verify(e=>e.SendEmailAsync("a@b.com",It.IsAny<string>(),It.IsAny<string>()),Times.Once);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ShouldFail()
    {
        var dto = new RegisterDto{Email="dup@x.com",Password="p",FirstName="F",LastName="L"};
        await _service.RegisterAsync(dto);
        var second = await _service.RegisterAsync(dto);
        second.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task ConfirmEmail_ShouldActivateUser()
    {
        var dto = new RegisterDto{Email="c@d.com",Password="p",FirstName="C",LastName="D"};
        await _service.RegisterAsync(dto);
        var user = await _context.Users.FirstAsync(u=>u.Email=="c@d.com");
        var ok = await _service.ConfirmEmailAsync(user.Id.ToString(),user.ConfirmationToken!);
        ok.Should().BeTrue();
        (await _context.Users.FindAsync(user.Id))!.IsEmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public async Task Login_Success_ShouldReturnToken()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("secret");
        _context.Users.Add(new User{Id=10,Email="login@test.com",FirstName="L",LastName="T",PasswordHash=hash,IsEmailConfirmed=true});
        await _context.SaveChangesAsync();

        var res = await _service.LoginAsync(new LoginDto{Email="login@test.com",Password="secret"});
        res.Succeeded.Should().BeTrue();
        res.Token.Should().Be("test-token");
    }

    [Fact]
    public async Task Login_Invalid_ShouldFail()
    {
        var res = await _service.LoginAsync(new LoginDto{Email="none@test.com",Password="x"});
        res.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task PasswordReset_RequestNonexistent_ShouldSucceedGeneric()
    {
        var res = await _service.RequestPasswordResetAsync("no@user.com");
        res.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task ResetPassword_InvalidToken_ShouldFail()
    {
        var dto = new RegisterDto{Email="r@t.com",Password="p",FirstName="R",LastName="T"};
        await _service.RegisterAsync(dto);
        var fail = await _service.ResetPasswordAsync(new ResetPasswordDto{Email="r@t.com",Token="bad",NewPassword="newp"});
        fail.Succeeded.Should().BeFalse();
    }
} 