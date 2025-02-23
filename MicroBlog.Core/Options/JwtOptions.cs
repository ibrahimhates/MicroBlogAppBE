namespace MicroBlog.Core.Options;

public class JwtOptions
{
    public string SecretKey { get; init; }
    public string Issuer { get; init; }
    public string Audience { get; init; }
    public int Expires { get; init; }
}