using MicroBlog.Core.Entities.Base;

namespace MicroBlog.Core.Entities;

public class User : BaseEntity,IEntity
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string UserName { get; set; }
    public byte[]? ProfilePicture { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string PasswordHash { get; set; }
    public bool IsActive { get; set; }
    public UserToken UserToken { get; set; }
}