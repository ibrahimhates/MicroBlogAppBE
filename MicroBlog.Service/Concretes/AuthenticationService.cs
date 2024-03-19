using AutoMapper;
using Azure;
using MicroBlog.Core.Abstractions.Jwt;
using MicroBlog.Core.Abstractions.Repositories;
using MicroBlog.Core.Abstractions.Services;
using MicroBlog.Core.Dtos.AuthDto.Request;
using MicroBlog.Core.Dtos.AuthDto.Response;
using MicroBlog.Core.Entities;
using MicroBlog.Core.Hash;
using MicroBlog.Core.ResponseResult.Dtos;
using MicroBlog.Repository.UnitOfWork;

namespace MicroBlog.Service.Concretes;

public class AuthenticationService : IAuthenticationService
{
    private readonly IAuthenticationRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _provider;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    
    public AuthenticationService(IAuthenticationRepository repository,
        IPasswordHasher passwordHasher, IJwtProvider provider, IMapper mapper, 
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _provider = provider;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<Core.ResponseResult.Response<UserTokenResponse>> LoginUserRequestAsync(LoginRequest loginRequest)
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

        var token = _provider.Generate(user);
        // todo Token dogrulama db ile yapilacak tam burda
        
        return Core.ResponseResult.Response<UserTokenResponse>
            .Success(new UserTokenResponse(token,null), 200);
    }

    public async Task<Core.ResponseResult.Response<UserTokenResponse>> RegisterUserRequestAsync(
        RegisterRequest registerRequest)
    {
        if (!registerRequest.Password.Equals(registerRequest.PasswordConfirm))
        {
            return Core.ResponseResult.Response<UserTokenResponse>
                .Fail($"User password and passwordConfirm does not matching", 400);
        }
        
        var user = _mapper.Map<User>(registerRequest);
        user.PasswordHash = _passwordHasher.Hash(registerRequest.Password);

        await _repository.CreateAsync(user);
        await _unitOfWork.SaveAsync();

        var token = _provider.Generate(user);
        
        return Core.ResponseResult.Response<UserTokenResponse>
            .Success(new UserTokenResponse(token,null), 200);
    }

    public Task<Core.ResponseResult.Response<NoContent>> ChangePasswordRequestAsync(
        ChangePasswordRequest changePasswordRequest)
    {
        throw new NotImplementedException();
    }
}