using MicroBlog.Core.Behaviour;

namespace MicroBlog.Core.Dtos.AuthDto.Request;

public record ForgetPasswordRequest(
        string UserNameOrEmail
    );