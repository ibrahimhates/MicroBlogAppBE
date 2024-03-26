using System.Security.Cryptography;
using AutoMapper;
using MicroBlog.Core.Abstractions.EmailSendProcedure;
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
using MicroBlog.Service.Logger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MicroBlog.Service.Concretes;

public class AuthenticationService : IAuthenticationService
{
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IAuthenticationRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _provider;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IUserTokenRepository _userTokenRepository;
    private readonly IQueueService _queueService;

    public AuthenticationService(IAuthenticationRepository repository,
        IPasswordHasher passwordHasher, IJwtProvider provider, IMapper mapper,
        IUnitOfWork unitOfWork, LinkGenerator linkGenerator,
        IHttpContextAccessor contextAccessor,
        IUserTokenRepository userTokenRepository,
        ILogger<AuthenticationService> logger,
        IQueueService queueService)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _provider = provider;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _linkGenerator = linkGenerator;
        _contextAccessor = contextAccessor;
        _userTokenRepository = userTokenRepository;
        _logger = logger;
        _queueService = queueService;
    }

    public async Task<Response<UserTokenResponse>> LoginUserRequestAsync(
        LoginRequest loginRequest)
    {
        int statusCode = 0;
        try
        {
            var user = await _repository.GetByEmailAsync(loginRequest.Email);

            if (user is not User)
            {
                statusCode = 404;
                throw new InvalidDataException($"User could not found. ");
            }

            if (!_passwordHasher.Verify(user.PasswordHash, loginRequest.Password))
            {
                statusCode = 401;
                throw new InvalidDataException($"User password does not matching. {loginRequest.Email}");
            }

            if (!user.VerifyEmail)
            {
                statusCode = 401;
                throw new InvalidDataException("Pending email verification. Please verify your email address");
            }

            UserToken userToken = user.UserToken;
            if (user.UserToken is not UserToken)
            {
                userToken = new();
            }

            // Kullacini hesabi pasif almissa tekrar giriste aktif olarak guncelleniyor 
            if (!user.IsActive) user.IsActive = true;

            var userTokenResponse = CreateToken(user, userToken, true);

            userToken.AccessToken = userTokenResponse.AccessToken;
            userToken.RefreshToken = userTokenResponse.RefreshToken;

            _repository.Update(user);
            _userTokenRepository.Update(userToken);
            await _unitOfWork.SaveAsync();

            return Response<UserTokenResponse>
                .Success(userTokenResponse, 200);
        }
        catch (InvalidDataException err)
        {
            _logger.SendWarning(err.Message);
            return Response<UserTokenResponse>
                .Fail(err.Message, statusCode);
        }
        catch (Exception err)
        {
            _logger.SendError(err);
            return Response<UserTokenResponse>
                .Fail("Something went wrong.", 500);
        }
    }

    public async Task<Response<UserTokenResponse>> RefreshTokenAsync(UserTokenResponse refreshRequestToken)
    {
        int statusCode = 0;
        try
        {
            statusCode = 200;
            var (result, userId) = await _provider.VerifyTokenAsync(refreshRequestToken.AccessToken);

            if (!result)
            {
                statusCode = 400;
                throw new InvalidDataException("Invalid Token.");
            }

            var user = await _repository
                .GetByCondition(x => x.Id == new Guid(userId), false)
                .Include(x => x.UserToken)
                .FirstOrDefaultAsync();

            if (user is not User)
            {
                statusCode = 404;
                throw new InvalidDataException("User could not found");
            }

            if (user.UserToken is not UserToken
                || string.IsNullOrEmpty(user.UserToken.RefreshToken)
                || !user.UserToken.RefreshToken.Equals(refreshRequestToken.RefreshToken)
                || user.UserToken.RefreshTokenExpires < DateTime.Now)
            {
                statusCode = 400;
                throw new InvalidDataException("Invalid Token.");
            }

            UserToken userToken = user.UserToken;

            var tokens = CreateToken(user, userToken, false);

            userToken.RefreshToken = tokens.RefreshToken;
            userToken.AccessToken = tokens.AccessToken;

            _repository.Update(user);
            _userTokenRepository.Update(userToken);
            await _unitOfWork.SaveAsync();

            return Response<UserTokenResponse>
                .Success(tokens, 200);
        }
        catch (InvalidDataException err)
        {
            _logger.SendWarning(err.Message);
            return Response<UserTokenResponse>.Fail(err.Message, statusCode);
        }
        catch (Exception err)
        {
            _logger.SendError(err);
            return Response<UserTokenResponse>
                .Fail("Something went wrong.", 500);
        }
    }

    public async Task<Response<NoContent>> RegisterUserRequestAsync(
        RegisterRequest registerRequest)
    {
        try
        {
            if (!registerRequest.Password.Equals(registerRequest.PasswordConfirm))
            {
                throw new InvalidDataException("User password and passwordConfirm does not matching");
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
                await _queueService.PublishAsync(new(
                    To: user.Email,
                    Body: confirmationLink,
                    EmailSendType: EmailSendType.VerifyEmail
                ));
            }
            catch (Exception)
            {
                throw new InvalidOperationException("An error was encountered while sending the verification mail." +
                                                    " Please try mail verification manually again later.");
            }

            return Response<NoContent>
                .Success($"User created successfully & Email sent to {user.Email}", 202);
        }
        catch (InvalidDataException err)
        {
            _logger.SendWarning(err.Message);
            return Response<NoContent>
                .Fail(err.Message, 400);
        }
        catch (InvalidOperationException err)
        {
            _logger.SendError(err);
            return Response<NoContent>
                .Fail(err.Message, 500);
        }
        catch (Exception err)
        {
            _logger.SendError(err);
            return Response<NoContent>
                .Fail("Something went wrong.", 500);
        }
    }

    public async Task<Response<NoContent>> VerifyUserRequestAsync(VerifyUserRequestDto verifyUserRequestDto)
    {
        int statusCode = 200;
        try
        {
            var user = await _repository.GetByEmailAsync(verifyUserRequestDto.email);

            if (user is not User)
            {
                statusCode = 404;
                throw new InvalidDataException($"User could not found");
            }

            if (user.VerifyEmail)
            {
                statusCode = 400;
                throw new InvalidDataException($"User is already verify");
            }

            if (string.IsNullOrEmpty(user.EmailVerifyToken)
                || !user.EmailVerifyToken.Equals(verifyUserRequestDto.token))
            {
                statusCode = 400;
                throw new InvalidDataException($"Invalid token");
            }

            user.VerifyEmail = true;
            _repository.Update(user);

            await _unitOfWork.SaveAsync();

            return Response<NoContent>
                .Success($"User has been successfully verified", 200);
        }
        catch (InvalidDataException err)
        {
            _logger.SendWarning(err.Message);
            return Response<NoContent>
                .Fail(err.Message, statusCode);
        }
        catch (Exception err)
        {
            _logger.SendError(err);
            return Response<NoContent>
                .Fail("Something went wrong.", 500);
        }
    }

    public async Task<Response<NoContent>> ForgetPasswordRequestAsync(
        ForgetPasswordRequest forgetPasswordRequest)
    {
        int statusCode = 200;
        try
        {
            User? user;
            if (forgetPasswordRequest.RequestType is ForgetPasswordRequestType.EMAIL)
                user = await _repository.GetByEmailAsync(forgetPasswordRequest.ResetUserInfo);
            else
                user = await _repository.GetByUserNameAsync(forgetPasswordRequest.ResetUserInfo);

            if (user is not User)
            {
                statusCode = 404;
                throw new InvalidDataException($"User could not found");
            }

            if (user.PasswordResetCodeExpr > DateTime.Now)
            {
                statusCode = 400;
                throw new InvalidDataException("You cannot receive a new reset code yet");
            }

            var resetCode = GenerateResetCode();
            user.PasswordResetCode = resetCode;
            user.PasswordResetCodeExpr = DateTime.Now.AddMinutes(3);

            _repository.Update(user);
            await _unitOfWork.SaveAsync();

            try
            {
                await _queueService.PublishAsync(new(
                    To: user.Email,
                    Body: resetCode,
                    EmailSendType: EmailSendType.ResetPassword
                ));
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    "An error was encountered while sending the verification mail." +
                    " Please try again later.");
            }

            return Response<NoContent>
                .Success($"The verification code was successfully sent to the following email {user.Email}"
                    , 200);
        }
        catch (InvalidDataException err)
        {
            _logger.SendWarning(err.Message);
            return Response<NoContent>
                .Fail(err.Message, statusCode);
        }
        catch (InvalidOperationException err)
        {
            _logger.SendError(err);
            return Response<NoContent>
                .Fail(err.Message, 500);
        }
        catch (Exception err)
        {
            _logger.SendError(err);
            return Response<NoContent>
                .Fail("Something went wrong.", 500);
        }
    }

    public async Task<Response<UserTokenForgetPasswordResponse>> ForgetPasswordRequestAsync(
        ResetPasswordRequest resetPasswordRequest)
    {
        int statusCode = 200;
        try
        {
            var user = await _repository.GetByEmailAsync(resetPasswordRequest.email);
            if (user is not User)
            {
                statusCode = 404;
                throw new InvalidDataException("User could not found.");
            }

            if (string.IsNullOrEmpty(user.PasswordResetCode))
            {
                statusCode = 404;
                throw new InvalidDataException("Password reset code not found.");
            }

            if (user.PasswordResetCodeExpr < DateTime.Now)
            {
                statusCode = 400;
                throw new InvalidDataException("The verification code has expired. Please request new code.");
            }

            if (!user.PasswordResetCode.Equals(resetPasswordRequest.verifyCode))
            {
                statusCode = 400;
                throw new InvalidDataException("The verification code is invalid. Please request a new code.");
            }

            var token = await _repository.GenerateVerifyAndResetTokenAsync();

            user.PasswordResetToken = token;
            user.PasswordResetTokenExpr = DateTime.Now.AddHours(3);
            user.PasswordResetCodeExpr = DateTime.Now;
            user.PasswordResetCode = string.Empty;

            _repository.Update(user);
            await _unitOfWork.SaveAsync();

            return Response<UserTokenForgetPasswordResponse>
                .Success(new UserTokenForgetPasswordResponse(token), 200);
        }
        catch (InvalidDataException err)
        {
            _logger.SendWarning(err.Message);
            return Response<UserTokenForgetPasswordResponse>
                .Fail(err.Message, statusCode);
        }
        catch (Exception err)
        {
            _logger.SendError(err);
            return Response<UserTokenForgetPasswordResponse>
                .Fail("Something went wrong.", 500);
        }
    }

    public async Task<Response<NoContent>> ResetPasswordRequest(ChangePasswordWithResetRequest resetRequest)
    {
        int statusCode = 200;
        try
        {
            var user = await _repository.GetByEmailAsync(resetRequest.email);

            if (user is not User)
            {
                statusCode = 404;
                throw new InvalidDataException("User could not found");
            }

            if (string.IsNullOrEmpty(user.PasswordResetToken)
                || user.PasswordResetTokenExpr < DateTime.Now
                || !user.PasswordResetToken.Equals(resetRequest.token))
            {
                statusCode = 400;
                throw new InvalidDataException("Invalid token");
            }

            if (!resetRequest.newPassword.Equals(resetRequest.newPasswordConfirm))
            {
                statusCode = 400;
                throw new InvalidDataException("User new password and new passwordConfirm does not matching");
            }

            if (_passwordHasher.Verify(user.PasswordHash, resetRequest.newPassword))
            {
                statusCode = 400;
                throw new InvalidDataException("The user's old password cannot be the same as the new password");
            }

            user.PasswordResetCodeExpr = DateTime.Now;
            user.PasswordResetToken = string.Empty;
            user.PasswordHash = _passwordHasher.Hash(resetRequest.newPassword);

            _repository.Update(user);
            await _unitOfWork.SaveAsync();

            return Response<NoContent>
                .Success($"User's password has been successfully changed", 200);
        }
        catch (InvalidDataException err)
        {
            _logger.SendWarning(err.Message);
            return Response<NoContent>
                .Fail(err.Message, statusCode);
        }
        catch (Exception err)
        {
            _logger.SendError(err);
            return Response<NoContent>
                .Fail("Something went wrong.", 500);
        }
    }

    public async Task<Response<NoContent>> ChangePasswordRequestAsync(
        string userId, ChangePasswordRequest chgPswRequest)
    {
        int statusCode = 200;
        try
        {
            var user = await _repository.GetByIdAsync(new Guid(userId));

            if (user is not User)
            {
                statusCode = 404;
                throw new InvalidDataException($"User could not found");
            }

            if (!_passwordHasher.Verify(user.PasswordHash, chgPswRequest.CurrentPassword))
            {
                statusCode = 400;
                throw new InvalidDataException($"User's old password does not match");
            }

            if (!chgPswRequest.NewPassword.Equals(chgPswRequest.ConfirmNewPassword))
            {
                statusCode = 400;
                throw new InvalidDataException($"User password and passwordConfirm does not matching");
            }

            if (_passwordHasher.Verify(user.PasswordHash, chgPswRequest.NewPassword))
            {
                statusCode = 400;
                throw new InvalidDataException($"The user's old password cannot be the same as the new password");
            }

            user.PasswordHash = _passwordHasher.Hash(chgPswRequest.NewPassword);

            _repository.Update(user);
            await _unitOfWork.SaveAsync();

            return Response<NoContent>
                .Success($"User's password has been successfully changed", 200);
        }
        catch (InvalidDataException err)
        {
            _logger.SendWarning(err.Message);
            return Response<NoContent>
                .Fail(err.Message, statusCode);
        }
        catch (Exception err)
        {
            _logger.SendError(err);
            return Response<NoContent>
                .Fail("Something went wrong.", 500);
        }
    }

    private UserTokenResponse CreateToken(User user, UserToken userToken, bool populateExp)
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