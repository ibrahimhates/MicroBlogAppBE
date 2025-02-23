using MicroBlog.Core.Entities.Base;

namespace MicroBlog.Core.Entities;

public class Follower : BaseEntity,IEntity<Guid>
{
    public Guid Id { get; set; }
    public Guid FollowerUserId { get; set; }
    public Guid UserId { get; set; }
    
    public User FollowerUser { get; set; }
    public User User { get; set; }
}