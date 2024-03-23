using MicroBlog.Core.Entities.Base;

namespace MicroBlog.Core.Entities;

public sealed class User : BaseEntity, IEntity
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string UserName { get; set; }
    public byte[]? ProfilePicture { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; }
    public string? OldEmail { get; set; }
    public string? EmailVerifyToken { get; set; }
    public string? PasswordResetCode { get; set; }
    public DateTime? PasswordResetCodeExpr { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime PasswordResetTokenExpr { get; set; }
    public bool VerifyEmail { get; set; }
    public string PhoneNumber { get; set; }
    public string PasswordHash { get; set; }
    public bool IsActive { get; set; }
    public UserToken UserToken { get; set; }
}