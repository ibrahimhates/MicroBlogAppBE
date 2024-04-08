using AutoMapper;
using MicroBlog.Core.Abstractions.Repositories;
using MicroBlog.Core.Abstractions.Services;
using MicroBlog.Core.Dtos.UserDto.Response;
using MicroBlog.Core.Entities;
using MicroBlog.Core.ResponseResult;
using MicroBlog.Service.Logger;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MicroBlog.Service.Concretes
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;
        private readonly IMapper _mapper;
        private readonly IFollowerRepository _followerRepository;
        public UserService(
            IUserRepository userRepository,
            ILogger<UserService> logger,
            IMapper mapper,
            IFollowerRepository followerRepository)
        {
            _userRepository=userRepository;
            _logger=logger;
            _mapper=mapper;
            _followerRepository=followerRepository;
        }

        public async Task<Response<List<UserFollowListResponse>>> GetUserFollowerListAsync(Guid id)
        {
            try
            {
                var users = await _followerRepository.GetByCondition(f => f.UserId == id, false)
                   .Include(f => f.FollowerUser)
                   .Select(f => f.FollowerUser)
                   .ToListAsync();

                var usersResponse = _mapper.Map<List<UserFollowListResponse>>(users);

                return Response<List<UserFollowListResponse>>
                    .Success(usersResponse, 200);
            }
            catch (Exception err)
            {
                _logger.SendError(err);
                return Response<List<UserFollowListResponse>>
                    .Fail("Something went wrong", 500);
            }
        }

        public async Task<Response<List<UserFollowListResponse>>> GetUserFollowingListAsync(Guid id)
        {
            try
            {
                var users = await _followerRepository.GetByCondition(f => f.FollowerUserId == id, false)
                   .Include(f => f.User)
                   .Select(f => f.User)
                   .ToListAsync();

                var usersResponse = _mapper.Map<List<UserFollowListResponse>>(users);

                return Response<List<UserFollowListResponse>>
                    .Success(usersResponse, 200);
            }
            catch (Exception err)
            {
                _logger.SendError(err);
                return Response<List<UserFollowListResponse>>
                    .Fail("Something went wrong", 500);
            }
        }

        public async Task<Response<UserInfoResponse>> GetUserInfoAsync(Guid id)
        {
            try
            {
                var user = await _userRepository.GetByCondition(u => u.Id == id, false)
                    .Include(u => u.Followers)
                    .Include(u => u.Followings)
                    .FirstOrDefaultAsync();

                if (user is not User)
                {
                    throw new InvalidDataException($"User could not found with id: {id}");
                }

                var userInfo = _mapper.Map<UserInfoResponse>(user);

                return Response<UserInfoResponse>.Success(userInfo, 200);
            }
            catch (InvalidDataException err)
            {
                _logger.SendWarning(err.Message);
                return Response<UserInfoResponse>
                    .Fail(err.Message, StatusCodes.Status404NotFound);
            }
            catch (Exception err)
            {
                _logger.SendError(err);
                return Response<UserInfoResponse>.Fail("Something went wrong", 500);
            }
        }
    }
}
