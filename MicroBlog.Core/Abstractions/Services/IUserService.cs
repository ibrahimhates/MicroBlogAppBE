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
    }
}
