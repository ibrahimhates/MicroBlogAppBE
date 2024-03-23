namespace MicroBlog.Core.Dtos.AuthDto.Request;

public record ChangePasswordWithResetRequest(
    string email,
    string token,
    string newPassword,
    string newPasswordConfirm
);