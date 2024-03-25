using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace MicroBlog.Service.Logger;

public static class LoggerExtensions
{
    
    // todo elastic yapilandirmasi buraya
    
    public static void SendError<T>(this ILogger<T> _logger, Exception ex, 
        [CallerMemberName] string methodName = "")
    {
        _logger.LogError($"{methodName} throw an exception. Exception Message: {ex.Message}. " +
                         "Inner Exception Message:" +
                         $" {(ex.InnerException != null ? ex.InnerException.Message : "")}");
    }
    
    public static void SendError<T>(this ILogger<T> _logger, string message, Exception ex, 
        [CallerMemberName] string methodName = "")
    {
        _logger.LogError($"{methodName}: {message}. Exception Message: {ex.Message}. " +
                         $"Inner Exception Message: {(ex.InnerException != null ? ex.InnerException.Message : "")}");
    }
    
    public static void SendWarning<T>(this ILogger<T> _logger, object o, 
        [CallerMemberName] string methodName = "")
    {
        _logger.LogWarning($"{methodName} warning. {JsonSerializer.Serialize(o)}");
    }
    
    public static void SendWarning<T>(this ILogger<T> _logger, string message, 
        [CallerMemberName] string methodName = "")
    {
        _logger.LogWarning($"{methodName} has been triggered . Message: {message}");
    }
    
    public static void SendInformation<T>(this ILogger<T> _logger, object o, 
        [CallerMemberName] string methodName = "")
    {
        _logger.LogInformation($"{methodName} just started. Request: " +
                               $"{JsonSerializer.Serialize(o)}");
    }
    
    public static void SendInformation<T>(this ILogger<T> _logger, string message, 
        [CallerMemberName] string methodName = "")
    {
        _logger.LogInformation($"{methodName}. {message}");
    }
}