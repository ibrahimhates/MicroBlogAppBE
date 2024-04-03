namespace MicroBlog.Core.Dtos.AuthDto.Request;

public record ChangePasswordWithResetRequest(
    Guid userIdentifier,
    string token,
    string newPassword,
    string newPasswordConfirm
);