using MicroBlog.Core.Entities;
using MicroBlog.Core.Repositories.GenericRepo;

namespace MicroBlog.Core.Abstractions.Repositories;

public interface IUserTokenRepository : IGenericRepository<UserToken>
{
}