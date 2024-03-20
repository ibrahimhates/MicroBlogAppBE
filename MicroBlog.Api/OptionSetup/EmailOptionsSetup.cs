using MicroBlog.Service.Concretes.EMailService;
using Microsoft.Extensions.Options;

namespace MicroBlogAppBE.OptionSetup;

public class EmailOptionsSetup : IConfigureOptions<EMailOptions>
{
    private const string SectionName = "Email";
    private readonly IConfiguration _configuration;

    public EmailOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(EMailOptions options)
    {
        _configuration.GetSection(SectionName).Bind(options);
    }
}