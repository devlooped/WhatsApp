using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Devlooped.WhatsApp;

public partial class LoggingHandler(IWhatsAppHandler innerHandler, ILogger logger) : DelegatingHandler(innerHandler)
{
    JsonSerializerOptions options = JsonContext.DefaultOptions;

    /// <summary>Gets or sets JSON serialization options to use when serializing logging data.</summary>
    public JsonSerializerOptions JsonSerializerOptions
    {
        get => options;
        set => options = Throw.IfNull(value);
    }

    public override async Task HandleAsync(Message message, CancellationToken cancellation = default)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            if (logger.IsEnabled(LogLevel.Trace))
                LogInvokedSensitive(nameof(HandleAsync), AsJson(message, options));
            else
                LogInvoked(nameof(HandleAsync));
        }

        try
        {
            await base.HandleAsync(message, cancellation);
            if (logger.IsEnabled(LogLevel.Debug))
                LogCompleted(nameof(HandleAsync));
        }
        catch (OperationCanceledException)
        {
            LogInvocationCanceled(nameof(HandleAsync));
            throw;
        }
        catch (Exception ex)
        {
            LogInvocationFailed(nameof(HandleAsync), ex);
            throw;
        }
    }

    /// <summary>Serializes <paramref name="value"/> as JSON for logging purposes.</summary>
    static string AsJson<T>(T value, JsonSerializerOptions options)
    {
        if (options.TryGetTypeInfo(typeof(T), out var typeInfo) is true)
        {
            try
            {
                return JsonSerializer.Serialize(value, typeInfo);
            }
            catch
            {
            }
        }

        // If we're unable to get a type info for the value, or if we fail to serialize,
        // return an empty JSON object. We do not want lack of type info to disrupt application behavior with exceptions.
        return "{}";
    }

    [LoggerMessage(LogLevel.Debug, "{MethodName} invoked.")]
    private partial void LogInvoked(string methodName);

    [LoggerMessage(LogLevel.Trace, "{MethodName} invoked: {Message}.")]
    private partial void LogInvokedSensitive(string methodName, string message);

    [LoggerMessage(LogLevel.Debug, "{MethodName} completed.")]
    private partial void LogCompleted(string methodName);

    [LoggerMessage(LogLevel.Debug, "{MethodName} canceled.")]
    private partial void LogInvocationCanceled(string methodName);

    [LoggerMessage(LogLevel.Error, "{MethodName} failed.")]
    private partial void LogInvocationFailed(string methodName, Exception error);

}
