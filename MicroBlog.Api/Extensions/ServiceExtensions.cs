using MicroBlog.Core.Abstractions.Jwt;
using MicroBlog.Core.Abstractions.Repositories;
using MicroBlog.Core.Abstractions.Services;
using MicroBlog.Core.Hash;
using MicroBlog.Repository.Concretes;
using MicroBlog.Repository.Context;
using MicroBlog.Repository.UnitOfWork;
using MicroBlog.Service.Concretes;
using MicroBlog.Service.Concretes.Jwt;
using MicroBlog.Service.Mapping;
using MicroBlogAppBE.OptionSetup;
using Microsoft.AspNetCore.Identity;
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
        services.AddScoped<IJwtProvider, JwtProvider>();
    }
    
    public static void ConfigureEmailSetup(this IServiceCollection services)
    {
        services.ConfigureOptions<EmailOptionsSetup>();
    }
    
    public static void ConfigureAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(Mapper));
    }
    
    public static void ConfigureRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
    }

    public static void ConfigureServices(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
    }
}
