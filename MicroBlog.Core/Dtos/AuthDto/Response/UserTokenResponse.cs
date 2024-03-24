namespace MicroBlog.Core.Dtos.AuthDto.Response;

public record UserTokenResponse(
        string AccessToken,
        string RefreshToken
    );