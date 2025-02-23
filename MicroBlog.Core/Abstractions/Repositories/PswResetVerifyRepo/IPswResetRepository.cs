using MicroBlog.Core.Abstractions.Repositories.GenericRepo;
using MicroBlog.Core.Entities.PswResetVerify;
using MongoDB.Bson;

namespace MicroBlog.Core.Abstractions.Repositories.PswResetVerifyRepo;

public interface IPswResetRepository : IGenericMongoDbRepository<ResetVerify,ObjectId>
{
    Task UpdateAsync(ResetVerify resetVerify);
}