using MicroBlog.Core.Entities;
using MicroBlog.Core.Repositories.GenericRepo;

namespace MicroBlog.Core.Abstractions.Repositories;

public interface IAuthenticationRepository : IGenericRepository<User>
{
    Task<User> GetByEmailAsync(string email,bool trackChanges = false);
    Task<User> GetByUserNameAsync(string userName,bool trackChanges = false);
    Task<string> GenerateVerifyAndResetTokenAsync();
}