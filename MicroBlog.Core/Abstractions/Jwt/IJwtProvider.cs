using MicroBlog.Core.Entities;

namespace MicroBlog.Core.Abstractions.Jwt;

public interface IJwtProvider
{
    string Generate(User user);
}