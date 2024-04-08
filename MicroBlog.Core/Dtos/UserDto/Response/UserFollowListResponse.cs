using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroBlog.Core.Dtos.UserDto.Response
{
    public record UserFollowListResponse(
            Guid Id,
            string FullName,
            byte[]? ProfilePicture,
            string UserName
        );
}
