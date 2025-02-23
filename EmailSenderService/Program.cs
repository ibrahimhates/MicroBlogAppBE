using EmailSenderService;
using EmailSenderService.EmailService.Service;
using EmailSenderService.OptionSetup;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.ConfigureOptions<EmailOptionsSetup>();
builder.Services.AddTransient<IEMailSender, EMailSender>();

var host = builder.Build();
host.Run();