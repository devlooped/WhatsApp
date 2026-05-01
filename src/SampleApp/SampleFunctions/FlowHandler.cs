using System.Runtime.CompilerServices;
using System.Text.Json;
using Devlooped.WhatsApp.Flows;

namespace Devlooped.WhatsApp;

static class FlowExtensions
{
    public static WhatsAppHandlerBuilder UseFlowsDemo(this WhatsAppHandlerBuilder builder)
        => builder.Use((inner, services) => new FlowHandler(inner));

    class FlowHandler(IWhatsAppHandler inner) : DelegatingWhatsAppHandler(inner)
    {
        public override async IAsyncEnumerable<Response> HandleAsync(IEnumerable<IMessage> messages, [EnumeratorCancellation] CancellationToken cancellation = default)
        {
            if (messages.OfType<FlowDataRequest>().FirstOrDefault() is { } request)
            {
                // In this case, we are the sole handlers of this type of message, but 
                // data from the incoming message could be used to route appropriately.
                if (request.Flow == "data")
                    yield return MockData(request);
                else
                    yield return MockList(request);

                yield break;
            }
            else if (messages.OfType<InteractiveFlowMessage>().FirstOrDefault() is { } flowMessage)
            {
                // This is a flow message, we can mock the response
                yield return flowMessage.Reply(
                    $"""
                    ☑️ {flowMessage.Source.Flow} payload:
                    ```
                    {JsonSerializer.Serialize(flowMessage.Data, JsonContext.DefaultOptions)}
                    ```
                    """);

                yield break;
            }


            if (messages.OfType<ContentMessage>().FirstOrDefault() is not { } message ||
                message.Content is not TextContent text ||
                !text.Text.StartsWith("/flow ", StringComparison.OrdinalIgnoreCase))
            {
                await foreach (var response in base.HandleAsync(messages, cancellation).WithCancellation(cancellation))
                    yield return response;

                yield break;
            }

            var flow = text.Text[6..].Trim();

            // Switches automatically from flow_id to flow_name
            if (long.TryParse(flow, out var id))
                yield return message.CallToAction("Comenzar", "Comenzar", id, draft: true);
            else
                yield return message.CallToAction("Comenzar", "Comenzar", flow, draft: true);
        }

        Response MockData(FlowDataRequest flow) => flow.Screen switch
        {
            "welcome_screen" => flow.DataResponse("confirmation_screen", new
            {
                message = "Recibido: " + flow.Data.GetProperty("comment").GetString(),
            }),
            _ => flow.DataResponse("welcome_screen", new
            {
                agent = "list",
                service = flow.ServiceId,
                user = flow.UserId,
                flow = flow.Token.Flow,
            }),
        };

        Response MockList(FlowDataRequest flow) => flow.DataResponse("SELECT_LIST", new
        {
            lists = new[]
            {
                new
                {
                    id = "supermercado",
                    main_content = new { title = "Supermercado" },
                    on_click_action = new
                    {
                        name = "navigate",
                        next = new { type = "screen", name = "SUPERMARKET_SCREEN" },
                        payload = new { selected_list = "supermercado" }
                    }
                },
                new
                {
                    id = "carniceria",
                    main_content = new { title = "Carnicería" },
                    on_click_action = new
                    {
                        name = "navigate",
                        next = new { type = "screen", name = "BUTCHER_SCREEN" },
                        payload = new { selected_list = "carniceria" }
                    }
                },
                new
                {
                    id = "ropa",
                    main_content = new { title = "Ropa" },
                    on_click_action = new
                    {
                        name = "navigate",
                        next = new { type = "screen", name = "CLOTHING_SCREEN" },
                        payload = new { selected_list = "ropa" }
                    }
                },
                new
                {
                    id = "ferreteria",
                    main_content = new { title = "Ferretería" },
                    on_click_action = new
                    {
                        name = "navigate",
                        next = new { type = "screen", name = "HARDWARE_SCREEN" },
                        payload = new { selected_list = "ferreteria" }
                    }
                }
            },
            items = new
            {
                supermercado = new[]
                {
                    new { id = "leche", title = "Leche entera 1L" },
                    new { id = "pan", title = "Pan integral" },
                    new { id = "huevos", title = "Huevos docena" },
                    new { id = "arroz", title = "Arroz blanco 1kg" },
                    new { id = "pasta", title = "Pasta spaghetti 500g" },
                    new { id = "aceite", title = "Aceite de oliva 500ml" },
                    new { id = "azucar", title = "Azúcar 1kg" },
                    new { id = "harina", title = "Harina 1kg" },
                    new { id = "sal", title = "Sal fina 500g" },
                    new { id = "cafe", title = "Café molido 250g" }
                },
                carniceria = new[]
                {
                    new { id = "carne_molida", title = "Carne molida 1kg" },
                    new { id = "pollo", title = "Pollo entero 2kg" },
                    new { id = "costilla", title = "Costilla de cerdo 1kg" },
                    new { id = "filete", title = "Filete de res 500g" },
                    new { id = "chorizo", title = "Chorizo artesanal 500g" },
                    new { id = "jamon", title = "Jamón serrano 200g" },
                    new { id = "salchicha", title = "Salchichas 12 unid" },
                    new { id = "pechuga", title = "Pechuga de pollo 1kg" }
                },
                ropa = new[]
                {
                    new { id = "camiseta", title = "Camiseta blanca M" },
                    new { id = "pantalon", title = "Pantalón vaquero talla 32" },
                    new { id = "zapatos", title = "Zapatos deportivos talla 42" },
                    new { id = "chaqueta", title = "Chaqueta de cuero L" },
                    new { id = "calcetines", title = "Calcetines pack 6 pares" },
                    new { id = "cinturon", title = "Cinturón de cuero negro" },
                    new { id = "sombrero", title = "Sombrero de lana" },
                    new { id = "bufanda", title = "Bufanda de invierno" }
                },
                ferreteria = new[]
                {
                    new { id = "martillo", title = "Martillo de carpintero" },
                    new { id = "destornillador", title = "Juego de destornilladores 6 piezas" },
                    new { id = "clavos", title = "Clavos 2 pulgadas 1kg" },
                    new { id = "tornillos", title = "Tornillos para madera 100 unid" },
                    new { id = "taladro", title = "Taladro eléctrico 500W" },
                    new { id = "pintura", title = "Pintura blanca 4L" },
                    new { id = "brocha", title = "Brocha de pintar 2 pulgadas" },
                    new { id = "cinta", title = "Cinta métrica 5m" },
                    new { id = "sierra", title = "Sierra manual" }
                }
            }
        });
    }
}
