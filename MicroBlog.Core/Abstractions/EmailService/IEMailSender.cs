
namespace MicroBlog.Core.Abstractions.EmailService;

public interface IEMailSender 
{
    Task SendMailVerifyAsync(string toEmail, string resetLink);
    Task SendForgetPasswordAsync(string toEmail, string verifyCode);
}