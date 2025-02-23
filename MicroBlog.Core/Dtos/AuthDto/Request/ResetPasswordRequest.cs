namespace MicroBlog.Core.Dtos.AuthDto.Request;

public record ResetPasswordRequest(
    Guid userIdentifier,
    string verifyCode);