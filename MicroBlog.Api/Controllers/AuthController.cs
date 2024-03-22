using MicroBlog.Core.Abstractions.Services;
using MicroBlog.Core.Dtos.AuthDto.Request;
using MicroBlogAppBE.Controllers.CustomControllerBase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MicroBlogAppBE.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : CustomController
{
    private readonly IAuthenticationService _service;

    public AuthController(IAuthenticationService service)
    {
        _service = service;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody]LoginRequest loginRequest)
    {
        var response = await _service.LoginUserRequestAsync(loginRequest);

        return CreateActionResultInstance(response);
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody]RegisterRequest registerRequest)
    {
        var response = await _service.RegisterUserRequestAsync(registerRequest);

        return CreateActionResultInstance(response);
    }

    [HttpGet("verifyEmail")]
    public async Task<IActionResult> VerifyEmail(string token,string email)
    {
        var response = await _service.VerifyUserRequestAsync(new (email, token));
        
        return CreateActionResultInstance(response);
    }

    [Authorize]
    [HttpGet("test")]
    public async Task<IActionResult> Test()
    {
        return Ok("Hello World");
    }
}