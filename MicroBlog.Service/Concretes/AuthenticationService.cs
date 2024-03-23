using AutoMapper;
using MicroBlog.Core.Abstractions.EmailService;
using MicroBlog.Core.Abstractions.Jwt;
using MicroBlog.Core.Abstractions.Repositories;
using MicroBlog.Core.Abstractions.Services;
using MicroBlog.Core.Behaviour;
using MicroBlog.Core.Dtos.AuthDto.Request;
using MicroBlog.Core.Dtos.AuthDto.Response;
using MicroBlog.Core.Entities;
using MicroBlog.Core.Hash;
using MicroBlog.Core.ResponseResult;
using MicroBlog.Core.ResponseResult.Dtos;
using MicroBlog.Repository.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MicroBlog.Service.Concretes;

public class AuthenticationService : IAuthenticationService
{
    private readonly IAuthenticationRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _provider;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEMailSender _mailSender;
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _contextAccessor;

    public AuthenticationService(IAuthenticationRepository repository,
        IPasswordHasher passwordHasher, IJwtProvider provider, IMapper mapper,
        IUnitOfWork unitOfWork, IEMailSender mailSender, LinkGenerator linkGenerator,
        IHttpContextAccessor contextAccessor)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _provider = provider;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _mailSender = mailSender;
        _linkGenerator = linkGenerator;
        _contextAccessor = contextAccessor;
    }

    public async Task<Core.ResponseResult.Response<UserTokenResponse>> LoginUserRequestAsync(
        LoginRequest loginRequest)
    {
        var user = await _repository.GetByEmailAsync(loginRequest.Email);

        if (user is not User)
        {
            return Core.ResponseResult.Response<UserTokenResponse>
                .Fail($"User could not found", 404);
        }

        if (!_passwordHasher.Verify(user.PasswordHash, loginRequest.Password))
        {
            return Core.ResponseResult.Response<UserTokenResponse>
                .Fail($"User password does not matching", 401);
        }

        if (!user.VerifyEmail)
        {
            return Core.ResponseResult.Response<UserTokenResponse>
                .Fail($"Pending email verification. Please verify your email address", 401);
        }

        var token = _provider.Generate(user);

        if (user.UserToken is not UserToken)
        {
            user.UserToken = new();
        }

        user.UserToken.Token = token;
        // todo Simdilik null kalacak daha sonra refresh token olusturulursa parametre olarak verilecek
        //user.UserToken.RefreshToken = given_refreshToken;

        _repository.Update(user);
        await _unitOfWork.SaveAsync();

        return Core.ResponseResult.Response<UserTokenResponse>
            .Success(new UserTokenResponse(token, null), 200);
    }

    public async Task<Core.ResponseResult.Response<NoContent>> RegisterUserRequestAsync(
        RegisterRequest registerRequest)
    {
        if (!registerRequest.Password.Equals(registerRequest.PasswordConfirm))
        {
            return Core.ResponseResult.Response<NoContent>
                .Fail($"User password and passwordConfirm does not matching", 400);
        }

        var user = _mapper.Map<User>(registerRequest);
        user.PasswordHash = _passwordHasher.Hash(registerRequest.Password);


        var emailToken = await _repository.GenerateVerifyAndResetTokenAsync();
        var confirmationLink = GenerateLink(user.Email, emailToken);

        user.EmailVerifyToken = emailToken;
        await _repository.CreateAsync(user);

        await _unitOfWork.SaveAsync();

        try
        {
            await _mailSender.SendMailVerifyAsync(user.Email, confirmationLink);
        }
        catch (Exception e)
        {
            return Core.ResponseResult.Response<NoContent>
                .Fail("An error was encountered while sending the verification mail." +
                      " Please try mail verification manually again later.",
                    500);
        }

        return Core.ResponseResult.Response<NoContent>
            .Success($"User created successfully & Email sent to {user.Email}", 202);
    }

    public async Task<Response<NoContent>> VerifyUserRequestAsync(VerifyUserRequestDto verifyUserRequestDto)
    {
        var user = await _repository.GetByEmailAsync(verifyUserRequestDto.email);

        if (user is not User)
        {
            return Core.ResponseResult.Response<NoContent>
                .Fail($"User could not found", 404);
        }

        if (user.VerifyEmail)
        {
            return Core.ResponseResult.Response<NoContent>
                .Fail($"User is already verify", 400);
        }

        if (string.IsNullOrEmpty(user.EmailVerifyToken))
        {
            return Core.ResponseResult.Response<NoContent>
                .Fail($"Invalid token", 400);
        }

        if (!user.EmailVerifyToken.Equals(verifyUserRequestDto.token))
            return Core.ResponseResult.Response<NoContent>
                .Fail($"Invalid token", 400);

        user.VerifyEmail = true;
        _repository.Update(user);

        await _unitOfWork.SaveAsync();

        return Core.ResponseResult.Response<NoContent>
            .Success($"User has been successfully verified", 200);
    }

    public async Task<Response<NoContent>> ForgetPasswordRequestAsync(
        ForgetPasswordRequest forgetPasswordRequest)
    {
        User? user;
        if (forgetPasswordRequest.RequestType is ForgetPasswordRequestType.EMAIL)
            user = await _repository.GetByEmailAsync(forgetPasswordRequest.ResetUserInfo);
        else
            user = await _repository.GetByUserNameAsync(forgetPasswordRequest.ResetUserInfo);

        if (user is not User)
            return Core.ResponseResult.Response<NoContent>
                .Fail($"User could not found", 404);

        if (user.PasswordResetCodeExpr > DateTime.Now)
            return Core.ResponseResult.Response<NoContent>
                .Fail($"You cannot receive a new reset code yet", 400);

        var resetCode = GenerateResetCode();
        user.PasswordResetCode = resetCode;
        user.PasswordResetCodeExpr = DateTime.Now.AddMinutes(3);

        _repository.Update(user);
        await _unitOfWork.SaveAsync();

        try
        {
            await _mailSender.SendForgetPasswordAsync(user.Email, resetCode);
        }
        catch (Exception e)
        {
            return Core.ResponseResult.Response<NoContent>
                .Fail("An error was encountered while sending the verification mail." +
                      " Please try again later.",
                    500);
        }

        return Core.ResponseResult.Response<NoContent>
            .Success($"The verification code was successfully sent to the following email {user.Email}"
                , 200);
    }

    public async Task<Response<UserTokenForgetPasswordResponse>> ForgetPasswordRequestAsync(
        ResetPasswordRequest resetPasswordRequest)
    {
        var user = await _repository.GetByEmailAsync(resetPasswordRequest.email);
        if (user is not User)
            return Core.ResponseResult.Response<UserTokenForgetPasswordResponse>
                .Fail("User could not found", 404);

        if (string.IsNullOrEmpty(user.PasswordResetCode))
        {
            return Core.ResponseResult.Response<UserTokenForgetPasswordResponse>
                .Fail("Password reset code not found.", 404);
        }

        if (user.PasswordResetCodeExpr < DateTime.Now)
        {
            return Core.ResponseResult.Response<UserTokenForgetPasswordResponse>
                .Fail("The verification code has expired. Please request new code", 400);
        }

        if (!user.PasswordResetCode.Equals(resetPasswordRequest.verifyCode))
        {
            return Core.ResponseResult.Response<UserTokenForgetPasswordResponse>
                .Fail("The verification code is invalid. Please request a new code.", 400);
        }

        var token = await _repository.GenerateVerifyAndResetTokenAsync();
        user.PasswordResetToken = token;
        user.PasswordResetTokenExpr = DateTime.Now.AddHours(3);
        user.PasswordResetCodeExpr = DateTime.Now;
        user.PasswordResetCode = string.Empty;

        _repository.Update(user);
        await _unitOfWork.SaveAsync();

        return Core.ResponseResult.Response<UserTokenForgetPasswordResponse>
            .Success(new UserTokenForgetPasswordResponse(token), 404);
    }

    public async Task<Response<NoContent>> ResetPasswordRequest(ChangePasswordWithResetRequest resetRequest)
    {
        var user = await _repository.GetByEmailAsync(resetRequest.email);

        if (user is not User)
            return Core.ResponseResult.Response<NoContent>
                .Fail("User could not found", 404);

        if (string.IsNullOrEmpty(user.PasswordResetToken)
            || user.PasswordResetTokenExpr < DateTime.Now
            || !user.PasswordResetToken.Equals(resetRequest.token))
            return Core.ResponseResult.Response<NoContent>
                .Fail("Invalid token", 400);

        if (!resetRequest.newPassword.Equals(resetRequest.newPasswordConfirm))
            return Core.ResponseResult.Response<NoContent>
                .Fail($"User new password and new passwordConfirm does not matching", 400);

        if (_passwordHasher.Verify(user.PasswordHash, resetRequest.newPassword))
            return Core.ResponseResult.Response<NoContent>
                .Fail($"The user's old password cannot be the same as the new password", 400);

        user.PasswordResetCodeExpr = DateTime.Now;
        user.PasswordResetToken = string.Empty;
        user.PasswordHash = _passwordHasher.Hash(resetRequest.newPassword);

        _repository.Update(user);
        await _unitOfWork.SaveAsync();

        return Core.ResponseResult.Response<NoContent>
            .Success($"User's password has been successfully changed", 200);
    }

    public async Task<Core.ResponseResult.Response<NoContent>> ChangePasswordRequestAsync(
        string userId, ChangePasswordRequest chgPswRequest)
    {
        var user = await _repository.GetByIdAsync(new Guid(userId));

        if (user is not User)
            return Core.ResponseResult.Response<NoContent>
                .Fail($"User could not found", 404);

        if (!_passwordHasher.Verify(user.PasswordHash, chgPswRequest.CurrentPassword))
            return Core.ResponseResult.Response<NoContent>
                .Fail($"User's old password does not match", 400);

        if (!chgPswRequest.NewPassword.Equals(chgPswRequest.ConfirmNewPassword))
            return Core.ResponseResult.Response<NoContent>
                .Fail($"User password and passwordConfirm does not matching", 400);

        if (_passwordHasher.Verify(user.PasswordHash, chgPswRequest.NewPassword))
            return Core.ResponseResult.Response<NoContent>
                .Fail($"The user's old password cannot be the same as the new password", 400);

        user.PasswordHash = _passwordHasher.Hash(chgPswRequest.NewPassword);
        await _unitOfWork.SaveAsync();

        return Core.ResponseResult.Response<NoContent>
            .Success($"User's password has been successfully changed", 200);
    }

    private string? GenerateLink(string email, string token)
    {
        var confirmationLink = _linkGenerator.GetUriByAction(
            _contextAccessor.HttpContext,
            action: "VerifyEmail",
            controller: "Auth",
            values: new
            {
                token, email
            });

        return confirmationLink;
    }

    private string GenerateResetCode() =>
        new Random().Next(100000, 999999).ToString();
}