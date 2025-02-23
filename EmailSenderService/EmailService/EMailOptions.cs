namespace EmailSenderService.EmailService;

public class EMailOptions
{
    public string From { get; init; }
    public string Psw { get; init; }
    public string Host { get; init; }
    public int Port { get; init; }
}