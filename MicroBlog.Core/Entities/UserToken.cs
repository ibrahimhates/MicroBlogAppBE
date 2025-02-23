using MicroBlog.Core.Entities.Base;

namespace MicroBlog.Core.Entities;

public class UserToken : BaseEntity,IEntity<Guid>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime RefreshTokenExpires { get; set; }
}