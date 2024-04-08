using MicroBlog.Core.Abstractions.Repositories;
using MicroBlog.Core.Entities;
using MicroBlog.Repository.Concretes.GenericRepo;
using MicroBlog.Repository.Context;

namespace MicroBlog.Repository.Concretes
{
    public sealed class UserRepository : GenericRepository<User, Guid>, IUserRepository
    {
        public UserRepository(MicroBlogDbContext context) 
            : base(context)
        {
        }
    }
}
