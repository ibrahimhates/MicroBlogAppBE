using MicroBlog.Core.Options;
using Microsoft.Extensions.Options;

namespace MicroBlogAppBE.OptionSetup;

public class MongoDbOptionsSetup : IConfigureOptions<MongoDbOptions>
{
    private readonly IConfiguration _configuration;

    public MongoDbOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(MongoDbOptions options)
    {
        _configuration.GetSection("MongoDBConnection").Bind(options);
    }
}
