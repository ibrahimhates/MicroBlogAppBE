using System.Text;
using System.Text.Json;
using EmailSenderService.EmailService.Enum;
using EmailSenderService.EmailService.Service;
using EmailSenderService.EmailService.Tamplate;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EmailSenderService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IEMailSender _mailSender;

    public Worker(ILogger<Worker> logger, IEMailSender mailSender)
    {
        _logger = logger;
        _mailSender = mailSender;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost",
            UserName = "microblog_user",
            Password = "Pass0word!?",
            VirtualHost = "/"
        };

        var conn = await factory.CreateConnectionAsync(stoppingToken);

        var channel = await conn.CreateChannelAsync();
        await channel.ExchangeDeclareAsync("email-box-exchange", ExchangeType.Direct);

        await channel.QueueDeclareAsync(
            queue: "email-send",
            durable: true,
            exclusive: false);

        await channel.QueueBindAsync("email-send", "email-box-exchange", "email-add");

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += async (model, e) =>
        {
            var body = e.Body.ToArray();
            var jsonBody = Encoding.UTF8.GetString(body);
            var tamplate = JsonSerializer.Deserialize<EmailSendTemplate>(jsonBody);
            if (tamplate is not null)
            {
                if (tamplate.EmailSendType == EmailSendType.VerifyEmail)
                {
                    try
                    {
                        await _mailSender.SendMailVerifyAsync(tamplate.To, tamplate.Body);
                        _logger.LogInformation($"Email successfully sent to {tamplate.To}");
                    }
                    catch (Exception err)
                    {
                        _logger.LogError($"Mail sender has a error. {err}");
                    }
                }
            }
        };

        await channel.BasicConsumeAsync("email-send", true, consumer);
    }
}