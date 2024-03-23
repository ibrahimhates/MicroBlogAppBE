namespace MicroBlog.Core.Dtos.AuthDto.Request;

public record ResetPasswordRequest(
    string email,
    string verifyCode);