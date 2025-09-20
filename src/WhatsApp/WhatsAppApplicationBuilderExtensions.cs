using System.ComponentModel;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;

namespace Devlooped.WhatsApp;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class WhatsAppApplicationBuilderExtensions
{
    /// <summary>
    /// Adds required WhatsApp middleware to the functions worker application builder.
    /// </summary>
    public static IFunctionsWorkerApplicationBuilder UseWhatsApp(this IFunctionsWorkerApplicationBuilder builder)
        => builder.UseFunctionContextAccessor();
}
