namespace MicroBlog.Core.Dtos.AuthDto.Request;

public record VerifyUserRequestDto(
    string email,
    string token
);