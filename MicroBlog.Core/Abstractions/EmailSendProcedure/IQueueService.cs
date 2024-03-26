using MicroBlog.Core.Abstractions.EmailSendProcedure.Template;

namespace MicroBlog.Core.Abstractions.EmailSendProcedure;

public interface IQueueService
{
    Task PublishAsync(EmailSendTemplate template);
}