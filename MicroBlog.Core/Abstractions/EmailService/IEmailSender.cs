using MicroBlog.Core.Entities;
using MicroBlog.Core.Entities.Base;

namespace MicroBlog.Core.Abstractions.EmailService;

public interface IEmailSender<T> 
    where T : class , IEntity
{
    Task SendMailVerifyAsync(T user, string verifyCode);
    Task SendForgetPasswordAsync(T user, string verifyCode);
}