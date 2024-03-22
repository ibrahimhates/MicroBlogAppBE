using AutoMapper;
using MicroBlog.Core.Abstractions.EmailService;
using MicroBlog.Core.Abstractions.Jwt;
using MicroBlog.Core.Abstractions.Repositories;
using MicroBlog.Core.Abstractions.Services;
using MicroBlog.Core.Dtos.AuthDto.Request;
using MicroBlog.Core.Dtos.AuthDto.Response;
using MicroBlog.Core.Entities;
using MicroBlog.Core.Hash;
using MicroBlog.Core.ResponseResult;
using MicroBlog.Core.ResponseResult.Dtos;
using MicroBlog.Repository.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.IdentityModel.Tokens;

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
                .Fail($"User password does not matching", 400);
        }

        if (!user.VerifyEmail)
        {
            return Core.ResponseResult.Response<UserTokenResponse>
                .Fail($"Pending email verification. Please verify your email address", 401);
        }

        var token = _provider.Generate(user);
        // todo Token dogrulama db ile yapilacak tam burda
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


        var emailToken = await _repository.GenerateEmailVerifyTokenAsync();
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
                .Fail($"User is already verify", 208);// todo degistir 400
        }

        if (user.EmailVerifyToken.IsNullOrEmpty())
        {
            return Core.ResponseResult.Response<NoContent>
                .Fail($"Invalid token", 400);
        }
        
        if (user.EmailVerifyToken.Equals(verifyUserRequestDto.token))
        {
            user.VerifyEmail = true;
            _repository.Update(user);

            await _unitOfWork.SaveAsync();
            
            return Core.ResponseResult.Response<NoContent>
                .Fail($"user has been successfully verified", 200);
        }
        
        return Core.ResponseResult.Response<NoContent>
            .Fail($"Invalid token", 400);
    }

    public Task<Core.ResponseResult.Response<NoContent>> ChangePasswordRequestAsync(
        ChangePasswordRequest changePasswordRequest)
    {
        throw new NotImplementedException();
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
}