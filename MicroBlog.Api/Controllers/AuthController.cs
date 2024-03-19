using MicroBlog.Core.Abstractions.Services;
using MicroBlog.Core.Dtos.AuthDto.Request;
using MicroBlogAppBE.Controllers.CustomControllerBase;
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
}