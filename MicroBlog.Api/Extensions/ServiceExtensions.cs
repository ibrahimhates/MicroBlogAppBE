using System.Text;
using MicroBlog.Core.Abstractions.EmailSendProcedure;
using MicroBlog.Core.Abstractions.EmailService;
using MicroBlog.Core.Abstractions.Jwt;
using MicroBlog.Core.Abstractions.Repositories;
using MicroBlog.Core.Abstractions.Services;
using MicroBlog.Core.Hash;
using MicroBlog.Repository.Concretes;
using MicroBlog.Repository.Context;
using MicroBlog.Repository.UnitOfWork;
using MicroBlog.Service.Concretes;
using MicroBlog.Service.Concretes.EmailSendProcedure;
using MicroBlog.Service.Concretes.EMailService;
using MicroBlog.Service.Concretes.Jwt;
using MicroBlog.Service.Mapping;
using MicroBlogAppBE.OptionSetup;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace MicroBlogAppBE.Extensions;

public static class ServiceExtensions
{
    public static void ConfigureSqlContext(this IServiceCollection services,
        IConfiguration configuration) => services
        .AddDbContext<MicroBlogDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("sqlConnection"))
        );

    public static void ConfigureSwaggerGenSetup(this IServiceCollection services)
    {
        services.AddSwaggerGen();
        services.ConfigureOptions<SwaggerGenOptionsSetup>();
    }

    public static void ConfigureCorsPolicyDevelopment(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("MicroBlogAppCorsPolicy", config =>
                config.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
        });
    }

    public static void ConfigureCorsPolicyAnyEnvironment(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("MicroBlogAppCorsPolicy",
                config =>
                {
                    config.AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithOrigins("https://localhost:3000", "http://localhost:3000");
                });
        });
    }

    public static void ConfigureJwtSetup(this IServiceCollection services, IConfiguration configuration)
    {
        //services.ConfigureOptions<JwtBearerOptionsSetup>();
        var _jwtOptions = configuration.GetSection("Jwt");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _jwtOptions["Issuer"],
                    ValidAudience = _jwtOptions["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_jwtOptions["SecretKey"]))
                };
            });

        services.ConfigureOptions<JwtOptionsSetup>();
        services.AddScoped<IJwtProvider, JwtProvider>();
    }

    public static void ConfigureEmailServiceSetup(this IServiceCollection services)
    {
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<IQueueService, QueueService>();
    }

    public static void ConfigureAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(Mapper));
    }

    public static void ConfigureRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
        services.AddScoped<IUserTokenRepository, UserTokenRepository>();
    }

    public static void ConfigureServices(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
    }
}