using MicroBlog.Core.Abstractions.Services;
using MicroBlogAppBE.Controllers.CustomControllerBase;
using Microsoft.AspNetCore.Mvc;

namespace MicroBlogAppBE.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]s")]
    public class UserController : CustomController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService=userService;
        }

        [HttpGet("{id:Guid}/detail")]
        public async Task<IActionResult> GetUserInfo([FromRoute(Name = "id")] Guid id)
        {

            var response = await _userService.GetUserInfoAsync(id);

            return CreateActionResultInstance(response);
        }

        [HttpGet("{id:Guid}/followers")]
        public async Task<IActionResult> GetUserFollowerList([FromRoute(Name = "id")] Guid id)
        {

            var response = await _userService.GetUserFollowerListAsync(id);

            return CreateActionResultInstance(response);
        }

        [HttpGet("{id:Guid}/followings")]
        public async Task<IActionResult> GetUserFollowingList([FromRoute(Name = "id")] Guid id)
        {

            var response = await _userService.GetUserFollowingListAsync(id);

            return CreateActionResultInstance(response);
        }
    }
}
