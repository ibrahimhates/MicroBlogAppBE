using MicroBlog.Core.Entities.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MicroBlog.Core.Entities.PswResetVerify;

public class ResetVerify : IEntity<ObjectId>
{
    [BsonId]
    public ObjectId Id { get; set; }
    public string UserId { get; set; }
    public string AuthField { get; set; }
    public DateTime Expires { get; set; }
}