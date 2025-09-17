using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Devlooped.WhatsApp;

/// <summary>
/// Provides extensions for processing WhatsApp messages asynchronusly 
/// using the running app <see cref="TaskScheduler"/>.
/// </summary>
public static class TaskSchedulerProcessorExtensions
{
    /// <summary>
    /// Uses the default <see cref="TaskScheduler"/> (or a previously registered one)
    /// to process WhatsApp messages asynchronously without delegating to an external 
    /// system (like queues or event grid).
    /// </summary>
    public static WhatsAppHandlerBuilder UseTaskSchedulerProcessor(this WhatsAppHandlerBuilder builder)
    {
        Throw.IfNull(builder);

        if (builder.Services.FirstOrDefault(x => x.ServiceType == typeof(IMessageProcessor)) is { } processor)
            builder.Services.Remove(processor);

        builder.Services.TryAddSingleton(TaskScheduler.Default);
        builder.Services.AddSingleton<IMessageProcessor, TaskSchedulerMessageProcessor>();

        return builder;
    }

    class TaskSchedulerMessageProcessor(PipelineRunner runner, TaskScheduler scheduler) : IMessageProcessor
    {
        public Task EnqueueAsync(string json, CancellationToken cancellation = default)
        {
            _ = Task.Factory.StartNew(
                async () => await ProcessAsync(json),
                CancellationToken.None,
                TaskCreationOptions.DenyChildAttach,
                scheduler).Unwrap();

            return Task.CompletedTask;
        }

        async Task ProcessAsync(string json) => await runner.ProcessAsync(json);
    }
}