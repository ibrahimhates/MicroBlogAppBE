using MicroBlog.Core.Abstractions.Repositories;
using MicroBlog.Repository.Concretes;
using MicroBlog.Repository.Context;
using MicroBlogAppBE.OptionSetup;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace MicroBlogAppBE.Extensions;

public static class ServiceExtensions
{
    public static void ConfigureSqlContext(this IServiceCollection services,
        IConfiguration configuration) => services
        .AddDbContext<MicroBlogDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("sqlConnection"))
        );

    public static void ConfigureJwtSetup(this IServiceCollection services)
    {
        services.ConfigureOptions<JwtOptionsSetup>();
        services.ConfigureOptions<JwtBearerOptionsSetup>();
    }
    public static void ConfigureRepositories(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
    }

    public static void ConfigureServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();
    }
}
