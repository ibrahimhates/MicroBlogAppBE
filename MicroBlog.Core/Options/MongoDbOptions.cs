namespace MicroBlog.Core.Options;

public class MongoDbOptions
{
    public string DatabaseName { get; init; }
    public string ConnectionString { get; init; }
}