using EmailSenderService.EmailService.Enum;

namespace EmailSenderService.EmailService.Tamplate;

public record EmailSendTemplate(  
    string To,
    string Body,
    EmailSendType EmailSendType
);