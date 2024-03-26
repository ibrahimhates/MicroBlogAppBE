using System.Text;
using System.Text.Json;
using MicroBlog.Core.Abstractions.EmailSendProcedure;
using MicroBlog.Core.Abstractions.EmailSendProcedure.Template;
using RabbitMQ.Client;

namespace MicroBlog.Service.Concretes.EmailSendProcedure;

public class QueueService : IQueueService
{
    public async Task PublishAsync(EmailSendTemplate template)
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost",
            UserName = "microblog_user",
            Password = "Pass0word!?",
            VirtualHost = "/"
        };
        var conn = await factory.CreateConnectionAsync();

        var channel = await conn.CreateChannelAsync();
        await channel.ExchangeDeclareAsync("email-box-exchange", ExchangeType.Direct);
        await channel.QueueDeclareAsync(
            queue: "email-send",
            durable: true,
            exclusive: false);
        
        await channel.QueueBindAsync("email-send", "email-box-exchange", "email-add");
        
        var jsonBody = JsonSerializer.Serialize(template);
        var body = Encoding.UTF8.GetBytes(jsonBody);

        await channel.BasicPublishAsync(
            "email-box-exchange",
            "email-add",
            body: body);
    }
}