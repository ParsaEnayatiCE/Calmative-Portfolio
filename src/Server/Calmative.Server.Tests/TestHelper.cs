using AutoMapper;
using Calmative.Server.API.Data;
using Calmative.Server.API.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Calmative.Server.Tests;

public static class TestHelper
{
    public static ApplicationDbContext GetInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        var context = new ApplicationDbContext(options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        return context;
    }

    public static IMapper GetMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        return config.CreateMapper();
    }
} 