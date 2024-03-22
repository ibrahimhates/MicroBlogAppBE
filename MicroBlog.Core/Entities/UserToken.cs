using MicroBlog.Core.Entities.Base;

namespace MicroBlog.Core.Entities;

public class UserToken : BaseEntity,IEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; }
    public string? RefreshToken { get; set; }
}