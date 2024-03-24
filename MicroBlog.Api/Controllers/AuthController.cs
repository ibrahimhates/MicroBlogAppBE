using MicroBlog.Core.Abstractions.Services;
using MicroBlog.Core.Dtos.AuthDto.Request;
using MicroBlog.Core.Dtos.AuthDto.Response;
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
    
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        [FromBody]UserTokenResponse userTokenResponse)
    {
        var response = await _service.RefreshTokenAsync(userTokenResponse);

        return CreateActionResultInstance(response);
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody]RegisterRequest registerRequest)
    {
        var response = await _service.RegisterUserRequestAsync(registerRequest);

        return CreateActionResultInstance(response);
    }

    [Authorize]
    [HttpPost("changePassword")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest changePasswordRequest)
    {
        var userId  = GetUserId();
        var response = await _service.ChangePasswordRequestAsync(userId,changePasswordRequest);
        
        return CreateActionResultInstance(response);
    }
    
    [HttpPost("resetPassword")]
    public async Task<IActionResult> ResetPassword([FromBody] ForgetPasswordRequest forgetPasswordRequest)
    {
        var response = await _service.ForgetPasswordRequestAsync(forgetPasswordRequest);
        
        return CreateActionResultInstance(response);
    }
    
    [HttpPost("verifyResetCode")]
    public async Task<IActionResult> VerifyResetCode([FromBody] ResetPasswordRequest resetRequest)
    {
        var response = await _service.ForgetPasswordRequestAsync(resetRequest);
        
        return CreateActionResultInstance(response);
    }
    
    [HttpPost("changePasswordReset")]
    public async Task<IActionResult> ChangePasswordReset([FromBody] ChangePasswordWithResetRequest resetRequest)
    {
        var response = await _service.ResetPasswordRequest(resetRequest);
        
        return CreateActionResultInstance(response);
    }
    
    [HttpGet("verifyEmail")]
    public async Task<IActionResult> VerifyEmail(string token,string email)
    {
        var response = await _service.VerifyUserRequestAsync(new (email, token));
        
        return CreateActionResultInstance(response);
    }

    [HttpGet("test"),Authorize]
    public async Task<IActionResult> TestEndpoint()
    {
        var email = GetUserEmail();
        
        return Ok($"Welcome back {email}");
    }
}