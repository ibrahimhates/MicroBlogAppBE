using System.Text.Json;
using MicroBlog.Core.Abstractions.Jwt;
using MicroBlog.Core.ResponseResult;
using MicroBlog.Core.ResponseResult.Dtos;
using MicroBlog.Service.Concretes.Jwt;
using Microsoft.AspNetCore.Http;

namespace MicroBlog.Service.Middleware;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private const string Separator = "/";
    private const string LoginPathName = "login";

    public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, IJwtProvider jwtProvider)
    {
        bool? isPathLogin = context.Request.Path.Value?
            .Split(Separator)
            .Last()
            .Equals(LoginPathName);

        if (isPathLogin.Value)
        {
            await _next(context);
            return;
        }

        var token = context.Request.Headers["Authorization"]
            .FirstOrDefault()?
            .Split(" ")
            .Last();

        if (string.IsNullOrEmpty(token))
        {
            await _next(context);
            return;
        }

        var (result,_) = await jwtProvider.VerifyTokenAsync(token);

        if (!result)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(Response<NoContent>.Fail("Invalid Token", 401))
            );
            return;
        }

        await _next(context);
    }
}