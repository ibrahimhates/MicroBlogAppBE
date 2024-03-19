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
            .FirstOrDefaultAsync();

        return user;
    }

    public async Task<User> GetByUserNameAsync(string userName,bool trackChanges = false)
    {
        var user = await GetByCondition(user => user.UserName == userName, trackChanges)
            .FirstOrDefaultAsync();

        return user;
    }
}