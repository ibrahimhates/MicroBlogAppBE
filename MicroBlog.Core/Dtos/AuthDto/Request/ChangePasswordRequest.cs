namespace MicroBlog.Core.Dtos.AuthDto.Request;

public record ChangePasswordRequest(
        string Email,
        string CurrentPassword,
        string NewPassword,
        string ConfirmNewPassword
    );