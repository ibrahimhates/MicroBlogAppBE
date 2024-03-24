using System.Text;
using MicroBlog.Service.Middleware;
using MicroBlogAppBE.Extensions;
using MicroBlogAppBE.OptionSetup;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Controller configure added
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

// Configure Swaggergensetup
builder.Services.ConfigureSwaggerGenSetup();

// Configure Sql connection string
builder.Services.ConfigureSqlContext(builder.Configuration);

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
}

app.UseMiddleware<JwtMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();