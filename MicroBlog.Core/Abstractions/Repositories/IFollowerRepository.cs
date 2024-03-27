using MicroBlog.Core.Entities;
using MicroBlog.Core.Repositories.GenericRepo;

namespace MicroBlog.Core.Abstractions.Repositories;

public interface IFollowerRepository : IGenericRepository<Follower,Guid>
{
}