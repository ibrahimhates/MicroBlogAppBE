using MicroBlog.Core.Behaviour;

namespace MicroBlog.Core.Abstractions.EmailSendProcedure.Template;

public record EmailSendTemplate(
        string To,
        string Body,
        EmailSendType EmailSendType
    );