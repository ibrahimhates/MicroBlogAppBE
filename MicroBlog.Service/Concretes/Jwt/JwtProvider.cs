using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MicroBlog.Core.Abstractions.Jwt;
using MicroBlog.Core.Abstractions.Repositories;
using MicroBlog.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MicroBlog.Service.Concretes.Jwt;

public sealed class JwtProvider : IJwtProvider
{
    private readonly JwtOptions _options;
    private readonly IUserTokenRepository _userTokenRepository;

    public JwtProvider(IOptions<JwtOptions> options,
        IUserTokenRepository userTokenRepository)
    {
        _userTokenRepository = userTokenRepository;
        _options = options.Value;
    }

    public string Generate(User user)
    {
        var claims = new Claim[]
        {
            new Claim("usrId", user.Id.ToString()),
            new Claim("contact", user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_options.SecretKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            null,
            DateTime.Now.AddHours(_options.Expires),
            signingCredentials);

        string tokenValue = new JwtSecurityTokenHandler()
            .WriteToken(token);

        return tokenValue;
    }

    public async Task<(bool, string?)> VerifyTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var userId = jwtToken.Claims.First(x => x.Type == "usrId").Value;

            var userToken = await _userTokenRepository
                .GetByCondition(u => u.UserId == new Guid(userId), false)
                .FirstOrDefaultAsync();

            if (userToken is null) return (false, null);

            tokenHandler.ValidateToken(userToken.AccessToken, new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _options.Issuer,
                ValidAudience = _options.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_options.SecretKey))
            }, out SecurityToken validatedToken);

            if (token.Equals(userToken.AccessToken)) return (true, userId);

            return (false, null);
        }
        catch (Exception err)
        {
            return (false, null);
        }
    }
}