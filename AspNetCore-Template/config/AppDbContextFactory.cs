using AspNetCore_Template.security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AspNetCore_Template.config;

public class AppDbContextFactory(ILoggedUserService loggedUserService) : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContextFactory() : this(new LoggedUserService(null))
    {
    }

    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));

        return new AppDbContext(optionsBuilder.Options, loggedUserService);
    }
}