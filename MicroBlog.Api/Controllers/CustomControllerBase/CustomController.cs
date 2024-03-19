using System.IdentityModel.Tokens.Jwt;
using FluentValidation.Results;
using MicroBlog.Core.ResponseResult;
using Microsoft.AspNetCore.Mvc;

namespace MicroBlogAppBE.Controllers.CustomControllerBase;

public class CustomController : ControllerBase
{
    public IActionResult CreateActionResultInstance<T>(Response<T> response)
    {
        if (response.StatusCode == 204)
        {
            return NoContent();
        }

        return new ObjectResult(response)
        {
            StatusCode = response.StatusCode
        };
    }
    
    public IActionResult CreateActionResultInstance<T>(ValidationResult validationResult)
    {
        return new ObjectResult(Response<T>.Fail(validationResult, 400))
        {
            StatusCode = 400
        };
    }
    
    [ApiExplorerSettings(IgnoreApi = true)]
    public string GetUserId()
    {
        if (HttpContext.User.Claims.Any())
            return HttpContext.User.Claims.First(f =>f.Type.Equals(JwtRegisteredClaimNames.Sub)).Value;
        
        return null;
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public string GetUserName()
    {
        if (HttpContext.User.Claims.Any())
            return HttpContext.User.Claims.First(f => f.Type.Equals(JwtRegisteredClaimNames.Name)).Value;
        
        return null;
    }
    
    [ApiExplorerSettings(IgnoreApi = true)]
    public string GetUserEmail()
    {
        if (HttpContext.User.Claims.Any())
            return HttpContext.User.Claims.First(f => f.Type.Equals(JwtRegisteredClaimNames.Email)).Value;
        
        return null;
    }
}