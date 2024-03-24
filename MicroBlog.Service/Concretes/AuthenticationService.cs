using System.Security.Cryptography;
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
using Microsoft.EntityFrameworkCore;

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
    private readonly IUserTokenRepository _userTokenRepository;
    public AuthenticationService(IAuthenticationRepository repository,
        IPasswordHasher passwordHasher, IJwtProvider provider, IMapper mapper,
        IUnitOfWork unitOfWork, IEMailSender mailSender, LinkGenerator linkGenerator,
        IHttpContextAccessor contextAccessor, IUserTokenRepository userTokenRepository)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _provider = provider;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _mailSender = mailSender;
        _linkGenerator = linkGenerator;
        _contextAccessor = contextAccessor;
        _userTokenRepository = userTokenRepository;
    }

    public async Task<Response<UserTokenResponse>> LoginUserRequestAsync(
        LoginRequest loginRequest)
    {
        var user = await _repository.GetByEmailAsync(loginRequest.Email);

        if (user is not User)
        {
            return Response<UserTokenResponse>
                .Fail($"User could not found", 404);
        }

        if (!_passwordHasher.Verify(user.PasswordHash, loginRequest.Password))
        {
            return Response<UserTokenResponse>
                .Fail($"User password does not matching", 401);
        }

        if (!user.VerifyEmail)
        {
            return Response<UserTokenResponse>
                .Fail($"Pending email verification. Please verify your email address", 401);
        }

        UserToken userToken = user.UserToken;
        if (user.UserToken is not UserToken)
        {
            userToken =  new();
        }
        
        // Kullacini hesabi pasif almissa tekrar giriste aktif olarak guncelleniyor 
        if (!user.IsActive) user.IsActive = true;

        var userTokenResponse = CreateToken(user, userToken,true);
        
        userToken.AccessToken = userTokenResponse.AccessToken;
        userToken.RefreshToken = userTokenResponse.RefreshToken;

        _repository.Update(user);
        _userTokenRepository.Update(userToken);
        await _unitOfWork.SaveAsync();

        return Response<UserTokenResponse>
            .Success(userTokenResponse, 200);
    }

    public async Task<Response<UserTokenResponse>> RefreshTokenAsync(UserTokenResponse refreshRequestToken)
    {
        var (result, userId) = await _provider.VerifyTokenAsync(refreshRequestToken.AccessToken);

        if (!result)
            return Response<UserTokenResponse>
                .Fail("Invalid Token", 400);

        var user = await _repository
            .GetByCondition(x => x.Id == new Guid(userId), false)
            .Include(x => x.UserToken)
            .FirstOrDefaultAsync();

        if (user is not User)
            return Response<UserTokenResponse>
                .Fail("User could not found", 404);
        
        if(user.UserToken is not UserToken 
           || string.IsNullOrEmpty(user.UserToken.RefreshToken) 
           || !user.UserToken.RefreshToken.Equals(refreshRequestToken.RefreshToken)
           || user.UserToken.RefreshTokenExpires < DateTime.Now)
            return Response<UserTokenResponse>
                .Fail("Invalid Token", 404);

        UserToken userToken = user.UserToken;
        
        var tokens = CreateToken(user,userToken, false);
        
        userToken.RefreshToken = tokens.RefreshToken;
        userToken.AccessToken= tokens.AccessToken;
        
        _repository.Update(user);
        _userTokenRepository.Update(userToken);
        await _unitOfWork.SaveAsync();
        
        return Response<UserTokenResponse>
            .Success(tokens, 200);
    }

    public async Task<Response<NoContent>> RegisterUserRequestAsync(
        RegisterRequest registerRequest)
    {
        if (!registerRequest.Password.Equals(registerRequest.PasswordConfirm))
        {
            return Response<NoContent>
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
            return Response<NoContent>
                .Fail("An error was encountered while sending the verification mail." +
                      " Please try mail verification manually again later.",
                    500);
        }

        return Response<NoContent>
            .Success($"User created successfully & Email sent to {user.Email}", 202);
    }

    public async Task<Response<NoContent>> VerifyUserRequestAsync(VerifyUserRequestDto verifyUserRequestDto)
    {
        var user = await _repository.GetByEmailAsync(verifyUserRequestDto.email);

        if (user is not User)
        {
            return Response<NoContent>
                .Fail($"User could not found", 404);
        }

        if (user.VerifyEmail)
        {
            return Response<NoContent>
                .Fail($"User is already verify", 400);
        }

        if (string.IsNullOrEmpty(user.EmailVerifyToken))
        {
            return Response<NoContent>
                .Fail($"Invalid token", 400);
        }

        if (!user.EmailVerifyToken.Equals(verifyUserRequestDto.token))
            return Response<NoContent>
                .Fail($"Invalid token", 400);

        user.VerifyEmail = true;
        _repository.Update(user);

        await _unitOfWork.SaveAsync();

        return Response<NoContent>
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
            return Response<NoContent>
                .Fail($"User could not found", 404);

        if (user.PasswordResetCodeExpr > DateTime.Now)
            return Response<NoContent>
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
            return Response<NoContent>
                .Fail("An error was encountered while sending the verification mail." +
                      " Please try again later.",
                    500);
        }

        return Response<NoContent>
            .Success($"The verification code was successfully sent to the following email {user.Email}"
                , 200);
    }

    public async Task<Response<UserTokenForgetPasswordResponse>> ForgetPasswordRequestAsync(
        ResetPasswordRequest resetPasswordRequest)
    {
        var user = await _repository.GetByEmailAsync(resetPasswordRequest.email);
        if (user is not User)
            return Response<UserTokenForgetPasswordResponse>
                .Fail("User could not found", 404);

        if (string.IsNullOrEmpty(user.PasswordResetCode))
        {
            return Response<UserTokenForgetPasswordResponse>
                .Fail("Password reset code not found.", 404);
        }

        if (user.PasswordResetCodeExpr < DateTime.Now)
        {
            return Response<UserTokenForgetPasswordResponse>
                .Fail("The verification code has expired. Please request new code", 400);
        }

        if (!user.PasswordResetCode.Equals(resetPasswordRequest.verifyCode))
        {
            return Response<UserTokenForgetPasswordResponse>
                .Fail("The verification code is invalid. Please request a new code.", 400);
        }

        var token = await _repository.GenerateVerifyAndResetTokenAsync();
        user.PasswordResetToken = token;
        user.PasswordResetTokenExpr = DateTime.Now.AddHours(3);
        user.PasswordResetCodeExpr = DateTime.Now;
        user.PasswordResetCode = string.Empty;

        _repository.Update(user);
        await _unitOfWork.SaveAsync();

        return Response<UserTokenForgetPasswordResponse>
            .Success(new UserTokenForgetPasswordResponse(token), 404);
    }

    public async Task<Response<NoContent>> ResetPasswordRequest(ChangePasswordWithResetRequest resetRequest)
    {
        var user = await _repository.GetByEmailAsync(resetRequest.email);

        if (user is not User)
            return Response<NoContent>
                .Fail("User could not found", 404);

        if (string.IsNullOrEmpty(user.PasswordResetToken)
            || user.PasswordResetTokenExpr < DateTime.Now
            || !user.PasswordResetToken.Equals(resetRequest.token))
            return Response<NoContent>
                .Fail("Invalid token", 400);

        if (!resetRequest.newPassword.Equals(resetRequest.newPasswordConfirm))
            return Response<NoContent>
                .Fail($"User new password and new passwordConfirm does not matching", 400);

        if (_passwordHasher.Verify(user.PasswordHash, resetRequest.newPassword))
            return Response<NoContent>
                .Fail($"The user's old password cannot be the same as the new password", 400);

        user.PasswordResetCodeExpr = DateTime.Now;
        user.PasswordResetToken = string.Empty;
        user.PasswordHash = _passwordHasher.Hash(resetRequest.newPassword);

        _repository.Update(user);
        await _unitOfWork.SaveAsync();

        return Response<NoContent>
            .Success($"User's password has been successfully changed", 200);
    }

    public async Task<Response<NoContent>> ChangePasswordRequestAsync(
        string userId, ChangePasswordRequest chgPswRequest)
    {
        var user = await _repository.GetByIdAsync(new Guid(userId));

        if (user is not User)
            return Response<NoContent>
                .Fail($"User could not found", 404);

        if (!_passwordHasher.Verify(user.PasswordHash, chgPswRequest.CurrentPassword))
            return Response<NoContent>
                .Fail($"User's old password does not match", 400);

        if (!chgPswRequest.NewPassword.Equals(chgPswRequest.ConfirmNewPassword))
            return Response<NoContent>
                .Fail($"User password and passwordConfirm does not matching", 400);

        if (_passwordHasher.Verify(user.PasswordHash, chgPswRequest.NewPassword))
            return Response<NoContent>
                .Fail($"The user's old password cannot be the same as the new password", 400);

        user.PasswordHash = _passwordHasher.Hash(chgPswRequest.NewPassword);
        await _unitOfWork.SaveAsync();

        return Response<NoContent>
            .Success($"User's password has been successfully changed", 200);
    }

    private UserTokenResponse CreateToken(User user,UserToken userToken, bool populateExp)
    {
        var accessToken = _provider.Generate(user);
        var refreshToken = GenerateRefreshToken();

        if (populateExp)
            userToken.RefreshTokenExpires = DateTime.Now.AddDays(7);

        return new(accessToken, refreshToken);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using (var rnd = RandomNumberGenerator.Create())
        {
            rnd.GetBytes(randomNumber);
        }

        return Convert.ToBase64String(randomNumber);
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