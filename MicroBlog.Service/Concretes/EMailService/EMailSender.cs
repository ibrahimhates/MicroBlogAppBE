using System.Net;
using System.Net.Mail;
using MicroBlog.Core.Abstractions.EmailService;
using MicroBlog.Core.Entities;
using Microsoft.Extensions.Options;

namespace MicroBlog.Service.Concretes.EMailService;

public class EMailSender : IEmailSender<User>
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

    public Task SendMailVerifyAsync(User user, string verifyCode)
    {
        return _client.SendMailAsync(
            new MailMessage(
                from: _options.From,
                to: user.Email,
                "Account Verification Code",
                $"You can use the following verification code to verify your account:" +
                $"\n\nVerification Code: [{verifyCode}]"));
    }

    public Task SendForgetPasswordAsync(User user, string verifyCode)
    {
        return _client.SendMailAsync(
            new MailMessage(
                from: _options.From,
                to: user.Email,
                "Password Reset Code",
                $"Your request to reset your password has been confirmed. " +
                $"Below is the verification code you need to use for the password reset process:" +
                $"\n\nVerification Code: [{verifyCode}]"));
    }
}