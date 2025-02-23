using EmailSenderService.EmailService;
using Microsoft.Extensions.Options;

namespace EmailSenderService.OptionSetup;

public class EmailOptionsSetup : IConfigureOptions<EMailOptions>
{
    private readonly IConfiguration _configuration;

    public EmailOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(EMailOptions options)
    {
        _configuration.GetSection("EMail").Bind(options);
    }
}