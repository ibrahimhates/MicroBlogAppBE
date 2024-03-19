using MicroBlogAppBE.Extensions;
using MicroBlogAppBE.OptionSetup;
using Microsoft.AspNetCore.Authentication.JwtBearer;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Sql connection string
builder.Services.ConfigureSqlContext(builder.Configuration);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

// JwtOptions and JwtBearerSetup configuration right here
builder.Services.ConfigureJwtSetup();

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

app.UseAuthentication();

app.UseAuthorization();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();