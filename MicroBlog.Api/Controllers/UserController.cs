using MicroBlog.Core.Abstractions.Services;
using MicroBlogAppBE.Controllers.CustomControllerBase;
using Microsoft.AspNetCore.Authorization;
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

        [Authorize]
        [HttpPost("{id:Guid}/follow")]
        public async Task<IActionResult> FollowAUser([FromRoute(Name = "id")] Guid id)
        {
            var userId = GetUserId();
            var response = await _userService.FollowAUserAsync(id,userId);

            return CreateActionResultInstance(response);
        }

        [Authorize]
        [HttpPost("{id:Guid}/unFollow")]
        public async Task<IActionResult> UnFollowAUser([FromRoute(Name = "id")] Guid id)
        {
            var userId = GetUserId();
            var response = await _userService.UnFollowAUserAsync(id, userId);

            return CreateActionResultInstance(response);
        }
        
        [Authorize]
        [HttpPost("{id:Guid}/removeFollower")]
        public async Task<IActionResult> RemoveFollower([FromRoute(Name = "id")] Guid id)
        {
            var userId = GetUserId();
            var response = await _userService.RemoveFollowerAsync(id, userId);

            return CreateActionResultInstance(response);
        }
    }
}
