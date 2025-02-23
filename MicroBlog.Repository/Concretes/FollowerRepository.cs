using MicroBlog.Core.Abstractions.Repositories;
using MicroBlog.Core.Abstractions.Repositories.GenericRepo;
using MicroBlog.Core.Entities;
using MicroBlog.Repository.Concretes.GenericRepo;
using MicroBlog.Repository.Context;

namespace MicroBlog.Repository.Concretes;

public class FollowerRepository : GenericRepository<Follower,Guid> , IFollowerRepository
{
    public FollowerRepository(MicroBlogDbContext context) : base(context)
    {
    }
}