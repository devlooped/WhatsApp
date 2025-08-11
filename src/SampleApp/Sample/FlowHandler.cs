using System.Runtime.CompilerServices;

namespace Devlooped.WhatsApp;

static class FlowExtensions
{
    public static WhatsAppHandlerBuilder UseFlowsDemo(this WhatsAppHandlerBuilder builder)
        => builder.Use((inner, services) => new FlowHandler(inner));

    class FlowHandler(IWhatsAppHandler inner) : DelegatingWhatsAppHandler(inner)
    {
        public override async IAsyncEnumerable<Response> HandleAsync(IEnumerable<IMessage> messages, [EnumeratorCancellation] CancellationToken cancellation = default)
        {
            if (messages.OfType<ContentMessage>().FirstOrDefault() is not { } message ||
                message.Content is not TextContent text ||
                !text.Text.StartsWith("/flow ", StringComparison.OrdinalIgnoreCase))
            {
                await foreach (var response in base.HandleAsync(messages, cancellation).WithCancellation(cancellation))
                    yield return response;

                yield break;
            }

            var flow = text.Text[6..].Trim();
            // We try parse as a number so we use flow_id vs flow_name
            object parameters = long.TryParse(flow, out _) ?
                new
                {
                    flow_message_version = 3,
                    flow_id = flow,
                    flow_cta = "Comenzar"
                } :
                new
                {
                    flow_message_version = 3,
                    flow_name = flow,
                    flow_cta = "Comenzar"
                };

            yield return Response.Create(message, async (client, token) => await client.SendAsync(message.Service.Id, new
            {
                messaging_product = "whatsapp",
                recipient_type = "individual",
                to = message.User.Number,
                type = "interactive",
                interactive = new
                {
                    type = "flow",
                    body = new
                    {
                        text = "Confirmar datos del recordatorio"
                    },
                    action = new
                    {
                        name = "flow",
                        parameters
                    }
                }
            }, token));
        }
    }
}
