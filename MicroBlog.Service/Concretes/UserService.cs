using AutoMapper;
using MicroBlog.Core.Abstractions.Repositories;
using MicroBlog.Core.Abstractions.Services;
using MicroBlog.Core.Dtos.UserDto.Response;
using MicroBlog.Core.Entities;
using MicroBlog.Core.ResponseResult;
using MicroBlog.Core.ResponseResult.Dtos;
using MicroBlog.Repository.UnitOfWork;
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
        private readonly IUnitOfWork _unitOfWork;

        public UserService(
            IUserRepository userRepository,
            ILogger<UserService> logger,
            IMapper mapper,
            IFollowerRepository followerRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository=userRepository;
            _logger=logger;
            _mapper=mapper;
            _followerRepository=followerRepository;
            _unitOfWork=unitOfWork;
        }

        public async Task<Response<NoContent>> FollowAUserAsync(Guid followingId, string followerId)
        {
            try
            {
                if (followerId.Equals(followingId.ToString()))
                {
                    throw new InvalidDataException($"You can't follow yourself.");
                }

                var result = await _userRepository.AnyAsync(x => x.Id == followingId);

                if (!result)
                {
                    throw new InvalidDataException($"The user to be followed could not be found: {followingId}");
                }

                var follower = new Follower()
                {
                    FollowerUserId = new Guid(followerId),
                    UserId = followingId,
                    IsDeleted = false,
                };

                await _followerRepository.CreateAsync(follower);

                await _unitOfWork.SaveAsync();

                return Response<NoContent>
                    .Success("The tracking process has been completed successfully.", 200);
            }
            catch (InvalidDataException err)
            {
                _logger.SendError(err);
                return Response<NoContent>
                    .Fail(err.Message, StatusCodes.Status400BadRequest);
            }
            catch (Exception err)
            {
                _logger.SendError(err);
                return Response<NoContent>
                    .Fail("Something went wrong.", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Response<NoContent>> UnFollowAUserAsync(Guid followingId, string followerId)
        {
            try
            {
                if (followerId.Equals(followingId.ToString()))
                {
                    throw new InvalidDataException($"You can't follow yourself.");
                }

                var result = await _userRepository.AnyAsync(x => x.Id == followingId);

                if (!result)
                {
                    throw new InvalidDataException($"No user to unfollow found: {followingId}");
                }

                var follower = await _followerRepository
                    .GetByCondition(x => x.UserId == followingId
                        && x.FollowerUserId == new Guid(followerId), false)
                    .FirstOrDefaultAsync();

                if (follower is null)
                {
                    throw new InvalidDataException($"You don't follow this user already: {followingId}");
                }

                _followerRepository.Delete(follower);

                await _unitOfWork.SaveAsync();

                return Response<NoContent>
                    .Success("Unfollow process completed successfully.", 200);
            }
            catch (InvalidDataException err)
            {
                _logger.SendError(err);
                return Response<NoContent>
                    .Fail(err.Message, StatusCodes.Status400BadRequest);
            }
            catch (Exception err)
            {
                _logger.SendError(err);
                return Response<NoContent>
                    .Fail("Something went wrong.", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Response<List<UserFollowListResponse>>> GetUserFollowerListAsync(Guid id)
        {
            try
            {
                var followers = await _followerRepository
                   .GetByCondition(f => f.UserId == id || f.FollowerUserId == id, false)
                   .Include(f => f.FollowerUser)
                   .ToListAsync();

                var users = followers.Where(f => f.UserId == id).Select(x => x.FollowerUser).ToList();
                var usersResponse = _mapper.Map<List<UserFollowListResponse>>(users);

                usersResponse.ForEach(f =>
                {
                    f.IsFollower = true;
                    f.IsFollowing = followers.Any(x => x.UserId == f.Id && x.FollowerUserId == id);
                });

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
                var followers = await _followerRepository
                   .GetByCondition(f => f.UserId == id || f.FollowerUserId == id, false)
                   .Include(f => f.User)
                   .ToListAsync();

                var users = followers.Where(f => f.FollowerUserId == id).Select(x => x.User).ToList();
                var usersResponse = _mapper.Map<List<UserFollowListResponse>>(users);

                usersResponse.ForEach(f =>
                {
                    f.IsFollower = followers.Any(x => x.FollowerUserId == f.Id && x.UserId == id); 
                    f.IsFollowing = true;
                });

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

        public async Task<Response<NoContent>> RemoveFollowerAsync(Guid followingId, string followerId)
        {

            try
            {
                if (followerId.Equals(followingId.ToString()))
                {
                    throw new InvalidDataException($"You can't follow yourself.");
                }

                var result = await _userRepository.AnyAsync(x => x.Id == followingId);

                if (!result)
                {
                    throw new InvalidDataException($"No user to unfollow found: {followingId}");
                }

                var follower = await _followerRepository
                    .GetByCondition(x => x.UserId == new Guid(followerId)
                        && x.FollowerUserId == followingId, false)
                    .FirstOrDefaultAsync();

                if (follower is null)
                {
                    throw new InvalidDataException($"This user is not following you anymore: {followingId}");
                }

                _followerRepository.Delete(follower);

                await _unitOfWork.SaveAsync();

                return Response<NoContent>
                    .Success("Unfollow process completed successfully.", 200);
            }
            catch (InvalidDataException err)
            {
                _logger.SendError(err);
                return Response<NoContent>
                    .Fail(err.Message, StatusCodes.Status400BadRequest);
            }
            catch (Exception err)
            {
                _logger.SendError(err);
                return Response<NoContent>
                    .Fail("Something went wrong.", StatusCodes.Status500InternalServerError);
            }
        }
    }
}
