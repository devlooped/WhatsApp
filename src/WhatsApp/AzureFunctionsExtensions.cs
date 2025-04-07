using System;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using Devlooped.WhatsApp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Azure.Functions.Worker.Builder;

/// <summary>
/// Extensions to register the WhatsApp handler for Azure Functions.
/// </summary>
public static class AzureFunctionsExtensions
{
    /// <summary>
    /// Configure the WhatsApp handler for Azure Functions.
    /// </summary>
    public static IFunctionsWorkerApplicationBuilder UseWhatsApp(this IFunctionsWorkerApplicationBuilder builder, Func<IServiceProvider, Message, Task> handler)
    {
        builder.Services.AddSingleton<IWhatsAppHandler>(services => new AnonymousWhatsAppHandler(services, handler));
        ConfigureServices(builder.Services);
        return builder;
    }

    static void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddSingleton<IWhatsAppClient, WhatsAppClient>();

        if (services.FirstOrDefault(x => x.ServiceType == typeof(QueueServiceClient)) == null)
        {
            services.AddSingleton(services => new QueueServiceClient(
                services.GetRequiredService<IConfiguration>()["AzureWebJobsStorage"]!,
                new QueueClientOptions
                {
#if DEBUG
                    Diagnostics =
                    {
                        IsLoggingEnabled = true,
                        IsLoggingContentEnabled = true,
                    },
#endif
                    MessageEncoding = QueueMessageEncoding.Base64
                }));
        }

        if (services.FirstOrDefault(x => x.ServiceType == typeof(TableServiceClient)) == null)
        {
            services.AddSingleton(services => new TableServiceClient(
                services.GetRequiredService<IConfiguration>()["AzureWebJobsStorage"]!,
                new TableClientOptions
                {
#if DEBUG
                    Diagnostics =
                    {
                        IsLoggingEnabled = true,
                        IsLoggingContentEnabled = true,
                    },
#endif
                }));
        }

        services.AddOptions<MetaOptions>()
            .BindConfiguration("Meta")
            .ValidateDataAnnotations();
    }

    /// <summary>
    /// Configure the WhatsApp handler for Azure Functions.
    /// </summary>
    public static IFunctionsWorkerApplicationBuilder UseWhatsApp<THandler>(this IFunctionsWorkerApplicationBuilder builder)
        where THandler : class, IWhatsAppHandler
    {
        builder.Services.AddSingleton<IWhatsAppHandler, THandler>();
        ConfigureServices(builder.Services);
        return builder;
    }

    /// <summary>
    /// Configure the WhatsApp handler for Azure Functions.
    /// </summary>
    public static IFunctionsWorkerApplicationBuilder UseWhatsApp<TService>(this IFunctionsWorkerApplicationBuilder builder, Func<TService, Message, Task> handler)
        where TService : notnull
    {
        builder.Services.AddSingleton<IWhatsAppHandler>(services => new AnonymousWhatsAppHandler<TService>(services.GetRequiredService<TService>(), handler));
        ConfigureServices(builder.Services);
        return builder;
    }

    /// <summary>
    /// Configure the WhatsApp handler for Azure Functions.
    /// </summary>
    public static IFunctionsWorkerApplicationBuilder UseWhatsApp<TService1, TService2>(this IFunctionsWorkerApplicationBuilder builder, Func<TService1, TService2, Message, Task> handler)
        where TService1 : notnull
        where TService2 : notnull
    {
        builder.Services.AddSingleton<IWhatsAppHandler>(services => new AnonymousWhatsAppHandler<TService1, TService2>(services.GetRequiredService<TService1>(), services.GetRequiredService<TService2>(), handler));
        ConfigureServices(builder.Services);
        return builder;
    }

    /// <summary>
    /// Configure the WhatsApp handler for Azure Functions.
    /// </summary>
    public static IFunctionsWorkerApplicationBuilder UseWhatsApp<TService1, TService2, TService3>(this IFunctionsWorkerApplicationBuilder builder, Func<TService1, TService2, TService3, Message, Task> handler)
        where TService1 : notnull
        where TService2 : notnull
        where TService3 : notnull
    {
        builder.Services.AddSingleton<IWhatsAppHandler>(services => new AnonymousWhatsAppHandler<TService1, TService2, TService3>(services.GetRequiredService<TService1>(), services.GetRequiredService<TService2>(), services.GetRequiredService<TService3>(), handler));
        ConfigureServices(builder.Services);
        return builder;
    }

    /// <summary>
    /// Configure the WhatsApp handler for Azure Functions.
    /// </summary>
    public static IFunctionsWorkerApplicationBuilder UseWhatsApp<TService1, TService2, TService3, TService4>(this IFunctionsWorkerApplicationBuilder builder, Func<TService1, TService2, TService3, TService4, Message, Task> handler)
        where TService1 : notnull
        where TService2 : notnull
        where TService3 : notnull
        where TService4 : notnull
    {
        builder.Services.AddSingleton<IWhatsAppHandler>(services => new AnonymousWhatsAppHandler<TService1, TService2, TService3, TService4>(services.GetRequiredService<TService1>(), services.GetRequiredService<TService2>(), services.GetRequiredService<TService3>(), services.GetRequiredService<TService4>(), handler));
        ConfigureServices(builder.Services);
        return builder;
    }

    /// <summary>
    /// Configure the WhatsApp handler for Azure Functions.
    /// </summary>
    public static IFunctionsWorkerApplicationBuilder UseWhatsApp<TService1, TService2, TService3, TService4, TService5>(this IFunctionsWorkerApplicationBuilder builder, Func<TService1, TService2, TService3, TService4, TService5, Message, Task> handler)
        where TService1 : notnull
        where TService2 : notnull
        where TService3 : notnull
        where TService4 : notnull
        where TService5 : notnull
    {
        builder.Services.AddSingleton<IWhatsAppHandler>(services => new AnonymousWhatsAppHandler<TService1, TService2, TService3, TService4, TService5>(services.GetRequiredService<TService1>(), services.GetRequiredService<TService2>(), services.GetRequiredService<TService3>(), services.GetRequiredService<TService4>(), services.GetRequiredService<TService5>(), handler));
        ConfigureServices(builder.Services);
        return builder;
    }

    /// <summary>
    /// Configure the WhatsApp handler for Azure Functions.
    /// </summary>
    public static IFunctionsWorkerApplicationBuilder UseWhatsApp<TService1, TService2, TService3, TService4, TService5, TService6>(this IFunctionsWorkerApplicationBuilder builder, Func<TService1, TService2, TService3, TService4, TService5, TService6, Message, Task> handler)
        where TService1 : notnull
        where TService2 : notnull
        where TService3 : notnull
        where TService4 : notnull
        where TService5 : notnull
        where TService6 : notnull
    {
        builder.Services.AddSingleton<IWhatsAppHandler>(services => new AnonymousWhatsAppHandler<TService1, TService2, TService3, TService4, TService5, TService6>(services.GetRequiredService<TService1>(), services.GetRequiredService<TService2>(), services.GetRequiredService<TService3>(), services.GetRequiredService<TService4>(), services.GetRequiredService<TService5>(), services.GetRequiredService<TService6>(), handler));
        ConfigureServices(builder.Services);
        return builder;
    }

    class AnonymousWhatsAppHandler(IServiceProvider services, Func<IServiceProvider, Message, Task> handler) : IWhatsAppHandler
    {
        public Task HandleAsync(Message message) => handler(services, message);
    }

    class AnonymousWhatsAppHandler<TService>(TService service, Func<TService, Message, Task> handler) : IWhatsAppHandler
    {
        public Task HandleAsync(Message message) => handler(service, message);
    }

    class AnonymousWhatsAppHandler<TService1, TService2>(TService1 service1, TService2 service2, Func<TService1, TService2, Message, Task> handler) : IWhatsAppHandler
    {
        public Task HandleAsync(Message message) => handler(service1, service2, message);
    }

    class AnonymousWhatsAppHandler<TService1, TService2, TService3>(TService1 service1, TService2 service2, TService3 service3, Func<TService1, TService2, TService3, Message, Task> handler) : IWhatsAppHandler
    {
        public Task HandleAsync(Message message) => handler(service1, service2, service3, message);
    }

    class AnonymousWhatsAppHandler<TService1, TService2, TService3, TService4>(TService1 service1, TService2 service2, TService3 service3, TService4 service4, Func<TService1, TService2, TService3, TService4, Message, Task> handler) : IWhatsAppHandler
    {
        public Task HandleAsync(Message message) => handler(service1, service2, service3, service4, message);
    }

    class AnonymousWhatsAppHandler<TService1, TService2, TService3, TService4, TService5>(TService1 service1, TService2 service2, TService3 service3, TService4 service4, TService5 service5, Func<TService1, TService2, TService3, TService4, TService5, Message, Task> handler) : IWhatsAppHandler
    {
        public Task HandleAsync(Message message) => handler(service1, service2, service3, service4, service5, message);
    }

    class AnonymousWhatsAppHandler<TService1, TService2, TService3, TService4, TService5, TService6>(TService1 service1, TService2 service2, TService3 service3, TService4 service4, TService5 service5, TService6 service6, Func<TService1, TService2, TService3, TService4, TService5, TService6, Message, Task> handler) : IWhatsAppHandler
    {
        public Task HandleAsync(Message message) => handler(service1, service2, service3, service4, service5, service6, message);
    }
}