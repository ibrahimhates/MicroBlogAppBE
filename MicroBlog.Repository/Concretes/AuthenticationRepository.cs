using System.Security.Cryptography;
using MicroBlog.Core.Abstractions.Repositories;
using MicroBlog.Core.Entities;
using MicroBlog.Repository.Concretes.GenericRepo;
using MicroBlog.Repository.Context;
using Microsoft.EntityFrameworkCore;

namespace MicroBlog.Repository.Concretes;

public class AuthenticationRepository :  GenericRepository<User>, IAuthenticationRepository
{
    public AuthenticationRepository(MicroBlogDbContext context) : base(context)
    {
    }

    public async Task<User> GetByEmailAsync(string email,bool trackChanges = false)
    {
        var user = await GetByCondition(user => user.Email == email, trackChanges)
            .Include(usr => usr.UserToken)
            .FirstOrDefaultAsync();

        return user;
    }

    public async Task<User> GetByUserNameAsync(string userName,bool trackChanges = false)
    {
        var user = await GetByCondition(user => user.UserName == userName, trackChanges)
            .Include(usr => usr.UserToken)
            .FirstOrDefaultAsync();

        return user;
    }

    public async Task<string> GenerateVerifyAndResetTokenAsync()
    {
        byte[] tokenBytes = GenerateRandomBytes(32);
        
        string token = Convert.ToBase64String(tokenBytes);
        
        return await Task.FromResult(token);
    }
    
    private byte[] GenerateRandomBytes(int length)
    {
        byte[] randomBytes = new byte[length];
        
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        
        return randomBytes;
    }
}