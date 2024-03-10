using MicroBlog.Core.Entities.BaseEntity;

namespace MicroBlog.Core.Entities;

public class UserToken : IEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; }
    public string RefreshToken { get; set; }
}