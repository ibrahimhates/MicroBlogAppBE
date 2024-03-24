using MicroBlog.Core.Dtos.AuthDto.Request;
using MicroBlog.Core.Dtos.AuthDto.Response;
using MicroBlog.Core.ResponseResult;
using MicroBlog.Core.ResponseResult.Dtos;

namespace MicroBlog.Core.Abstractions.Services;

public interface IAuthenticationService
{
    Task<Response<UserTokenResponse>> LoginUserRequestAsync(LoginRequest loginRequest);
    Task<Response<UserTokenResponse>> RefreshTokenAsync(UserTokenResponse refreshRequestToken);
    Task<Response<NoContent>> RegisterUserRequestAsync(RegisterRequest registerRequest);
    Task<Response<NoContent>> VerifyUserRequestAsync(VerifyUserRequestDto verifyUserRequestDto);
    Task<Response<NoContent>> ForgetPasswordRequestAsync(
        ForgetPasswordRequest forgetPasswordRequest);
    Task<Response<UserTokenForgetPasswordResponse>> ForgetPasswordRequestAsync(
        ResetPasswordRequest resetPasswordRequest);
    Task<Response<NoContent>> ResetPasswordRequest(ChangePasswordWithResetRequest resetRequest);
    Task<Response<NoContent>> ChangePasswordRequestAsync(string userId,ChangePasswordRequest chgPswRequest);
}