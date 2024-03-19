using MicroBlog.Core.Dtos.AuthDto.Request;
using MicroBlog.Core.Dtos.AuthDto.Response;
using MicroBlog.Core.ResponseResult;
using MicroBlog.Core.ResponseResult.Dtos;

namespace MicroBlog.Core.Abstractions.Services;

public interface IAuthenticationService
{
    Task<Response<UserTokenResponse>> LoginUserRequestAsync(LoginRequest loginRequest);
    
    Task<Response<UserTokenResponse>> RegisterUserRequestAsync(RegisterRequest registerRequest);
    
    //Task<Response<UserTokenForgetPasswordResponse>> ForgetPasswordRequestAsync(ForgetPasswordRequest loginRequest);
    
    Task<Response<NoContent>> ChangePasswordRequestAsync(ChangePasswordRequest changePasswordRequest);
}