using MicroBlog.Service.Middleware;
using MicroBlogAppBE.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.JwtSettings.json");

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

if (environment.Equals("Development"))
{
    builder
        .Configuration
        .AddJsonFile($"appsettings.Development.json"
            , true, true);
    
    builder.Services.ConfigureCorsPolicyDevelopment();
}
else
{
    builder
        .Configuration
        .AddJsonFile($"appsettings.{environment}.json", optional: false, reloadOnChange: true);
    builder.Services.AddLogging().AddSerilog();
    
    builder.Services.ConfigureCorsPolicyAnyEnvironment();
}

// Controller configure added
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

// Configure Swaggergensetup
builder.Services.ConfigureSwaggerGenSetup();

// Configure Sql connection string
builder.Services.ConfigureSqlContext(builder.Configuration);

// Mongo Db configure
builder.Services.ConfigureMongoDb();

// Configure AutoMapper
builder.Services.ConfigureAutoMapper();

// JwtOptions bind and JwtBearerSetup configuration 
builder.Services.ConfigureJwtSetup(builder.Configuration); // todo gecici olarak parametre aliyor

// Emailoptions Bind configuration
builder.Services.ConfigureEmailServiceSetup();

// repository configure Addscoped
builder.Services.ConfigureRepositories();

// services configure Addscoped
builder.Services.ConfigureServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // o =>
    // {
    //     o.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    // }
}

app.UseCors("MicroBlogAppCorsPolicy");

app.UseMiddleware<JwtMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();