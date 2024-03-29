using MicroBlog.Core.Abstractions.Repositories.PswResetVerifyRepo;
using MicroBlog.Core.Entities.PswResetVerify;
using MicroBlog.Core.Options;
using MicroBlog.Repository.Concretes.GenericRepo;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace MicroBlog.Repository.Concretes.PswResetVerifyRepo;

public class PswResetRepository : GenericMongoDbRepository<ResetVerify,ObjectId>, IPswResetRepository
{
    public PswResetRepository(IOptions<MongoDbOptions> options) 
        : base(options,"PswResetFields")
    {
        
    }

    public override async Task UpdateAsync(ResetVerify resetVerify)
    {
         var document = await GetByCondition(x => x.UserId == resetVerify.UserId);
        
        if (document is null)
        {
            await CreateAsync(resetVerify);
            return;
        }
        
        document.Expires = resetVerify.Expires;
        document.AuthField = resetVerify.AuthField;

        await base.UpdateAsync(document);
    }

}