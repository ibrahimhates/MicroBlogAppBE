namespace MicroBlog.Core.Dtos.AuthDto.Response;

public record UserTokenResponse(
        string Token,
        string RefreshToken
    );