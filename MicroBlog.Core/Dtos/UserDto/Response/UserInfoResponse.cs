
using MicroBlog.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace MicroBlog.Core.Dtos.UserDto.Response
{
    public record UserInfoResponse(
            Guid Id,
            string FullName,
            byte[]? ProfilePicture,
            string UserName
        )
    {
        public int Follower { get; init; }
        public int Following { get; init; }
        public int DailyCount { get; set; }
    }
}
