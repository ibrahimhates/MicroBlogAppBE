namespace MicroBlog.Core.Dtos.AuthDto.Request;

public record ChangePasswordRequest(
        string CurrentPassword,
        string NewPassword,
        string ConfirmNewPassword
    );