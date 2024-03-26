namespace EmailSenderService.EmailService.Service;

public interface IEMailSender
{
    Task SendMailVerifyAsync(string toEmail, string resetLink);
    Task SendForgetPasswordAsync(string toEmail, string verifyCode);
}