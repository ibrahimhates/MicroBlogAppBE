using MicroBlog.Core.Entities.BaseEntity;

namespace MicroBlog.Core.Entities;

public class User : IEntity
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string PasswordHash { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public UserToken UserToken { get; set; }
}