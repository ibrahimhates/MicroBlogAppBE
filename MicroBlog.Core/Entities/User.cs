using System.ComponentModel.DataAnnotations;
using MicroBlog.Core.Entities.Base;

namespace MicroBlog.Core.Entities;

public sealed class User : BaseEntity, IEntity<Guid>
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string UserName { get; set; }
    public byte[]? ProfilePicture { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    [EmailAddress]
    public string Email { get; set; }
    public string? OldEmail { get; set; }
    public string? EmailVerifyToken { get; set; }
    public bool VerifyEmail { get; set; }
    [Phone]
    public string? PhoneNumber { get; set; }
    public string PasswordHash { get; set; }
    public bool IsActive { get; set; }
    public UserToken UserToken { get; set; }
    public ICollection<Follower> Followers { get; set; }
    public ICollection<Follower> Followings { get; set; }
}