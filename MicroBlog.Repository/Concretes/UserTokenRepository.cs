using MicroBlog.Core.Abstractions.Repositories;
using MicroBlog.Core.Entities;
using MicroBlog.Repository.Concretes.GenericRepo;
using MicroBlog.Repository.Context;

namespace MicroBlog.Repository.Concretes;

public class UserTokenRepository : GenericRepository<UserToken>, IUserTokenRepository
{
    public UserTokenRepository(MicroBlogDbContext context) : base(context)
    {
    }
}