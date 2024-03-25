namespace MicroBlogAppBE.Extensions;

public static class EnvironmentServiceExtensions
{
    // public static void ConfigureEnvironment(this IServiceCollection services,IConfiguration config)
    // {
    //     var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    //     var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
    //     if (environment.Equals("Development"))
    //     {
    //         config.AddJsonFile($"appsettings.Development.json", true, true);
    //         services.AddCors(options =>
    //         {
    //             options.AddPolicy(MyAllowSpecificOrigins, builder =>
    //                 builder
    //                     .AllowAnyOrigin()
    //                     .AllowAnyMethod()
    //                     .AllowAnyHeader());
    //         });
    //
    //     }
    //     else
    //     {
    //         config.AddJsonFile($"appsettings.{environment}.json", optional: false, reloadOnChange: true);
    //         services.AddLogging().AddSerilog();
    //         services.AddCors(options =>
    //         {
    //             options.AddPolicy(name: MyAllowSpecificOrigins,
    //                 policy =>
    //                 {
    //                     policy.AllowAnyHeader()
    //                         .AllowAnyMethod()
    //                         .WithOrigins("https://localhost:3000", "http://localhost:3000");
    //                 });
    //         });
    //
    //     }
    // }
}