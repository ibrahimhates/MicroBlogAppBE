using MicroBlog.Core.Dtos.UserDto.Response;
using MicroBlog.Core.ResponseResult;
using MicroBlog.Core.ResponseResult.Dtos;

namespace MicroBlog.Core.Abstractions.Services
{
    public interface IUserService
    {
        Task<Response<UserInfoResponse>> GetUserInfoAsync(Guid id);
        Task<Response<List<UserFollowListResponse>>> GetUserFollowerListAsync(Guid id);
        Task<Response<List<UserFollowListResponse>>> GetUserFollowingListAsync(Guid id);
        Task<Response<NoContent>> FollowAUserAsync(Guid followingId, string followerId);
        Task<Response<NoContent>> UnFollowAUserAsync(Guid followingId, string followerId);
        Task<Response<NoContent>> RemoveFollowerAsync(Guid followingId, string followerId);
    }
}
