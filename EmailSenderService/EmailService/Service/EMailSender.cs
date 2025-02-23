using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace EmailSenderService.EmailService.Service;

public class EMailSender : IEMailSender
{
    private readonly EMailOptions _options;
    private readonly SmtpClient _client;

    public EMailSender(IOptions<EMailOptions> options)
    {
        _options = options.Value;
        _client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(_options.From, _options.Psw)
        };
    }

    public Task SendMailVerifyAsync(string toEmail, string resetLink)
    {
        return _client.SendMailAsync(
            new MailMessage(
                from: _options.From,
                to: toEmail,
                "Email Verification Link",
                $"You can use the following verification link to verify your account:" +
                $"\n\nYour single-use verification Link: {resetLink}"));
    }

    public Task SendForgetPasswordAsync(string toEmail, string verifyCode)
    {
        return _client.SendMailAsync(
            new MailMessage(
                from: _options.From,
                to: toEmail,
                "Password Reset Code",
                $"Your request to reset your password has been confirmed. " +
                $"Below is the verification code you need to use for the password reset process:" +
                $"\n\nYour single-use verification code: [{verifyCode}]"));
    }
}