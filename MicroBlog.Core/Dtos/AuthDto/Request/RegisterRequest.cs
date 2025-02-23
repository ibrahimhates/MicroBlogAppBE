namespace MicroBlog.Core.Dtos.AuthDto.Request;

public record RegisterRequest(
        string FirstName,
        string LastName,
        string UserName,
        string Password,
        string PasswordConfirm,
        string Email,
        string PhoneNumber,
        byte[]? ProfilePicture
    );