
namespace MicroBlog.Core.Dtos.UserDto.Response
{
    public record UserFollowListResponse(
            Guid Id,
            string FullName,
            byte[]? ProfilePicture,
            string UserName
        )
    {
        public bool IsFollower { get; set; } // benim takipcim mi
        public bool IsFollowing { get; set; } // benim takip ettigim mi
    }
}
