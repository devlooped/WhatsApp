using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Devlooped.WhatsApp;

public class WhatsAppClientTests(ITestOutputHelper output)
{
    [Fact]
    public async Task ThrowsIfNoConfiguredNumberAsync()
    {
        var client = WhatsAppClient.Create(MockHttpClientFactory.Default, new MetaOptions
        {
            VerifyToken = "asdf"
        }, MockLogger.Create<WhatsAppClient>());

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => client.SendAsync("1234", new { }));

        Assert.Equal("numberId", ex.ParamName);
    }

    [SecretsFact("Meta:VerifyToken", "SendFrom", "SendTo")]
    public async Task SendsMessageAsync()
    {
        var (configuration, client) = Initialize();

        var id = await client.SendAsync(configuration["SendFrom"]!, configuration["SendTo"]!, "Hi there!");

        Assert.NotNull(id);
        Assert.NotEmpty(id);
    }

    [SecretsFact("Meta:VerifyToken", "SendFrom", "SendTo")]
    public async Task ReactToSentMessageAsync()
    {
        var (configuration, client) = Initialize();

        var id = await client.SendAsync(configuration["SendFrom"]!, configuration["SendTo"]!, "Hi there!");

        Assert.NotNull(id);
        Assert.NotEmpty(id);

        await client.ReactAsync(configuration["SendFrom"]!, configuration["SendTo"]!, id, "🙏");
    }

    [SecretsFact("Meta:VerifyToken", "SendFrom", "SendTo")]
    public async Task ReplyToSentMessageAsync()
    {
        var (configuration, client) = Initialize();
        var from = configuration["SendFrom"]!;
        var to = configuration["SendTo"]!;

        var id = await client.SendAsync(configuration["SendFrom"]!, configuration["SendTo"]!, "Hi there!");

        Assert.NotNull(id);
        Assert.NotEmpty(id);

        var reply = await client.ReplyAsync(
            from,
            to,
            id,
            "Reply here!");

        Assert.NotNull(reply);
        Assert.NotEmpty(reply);

        Assert.NotEqual(id, reply);
    }

    [SecretsFact("Meta:VerifyToken", "SendFrom", "SendTo")]
    public async Task SendsButtonAsync()
    {
        var (configuration, client) = Initialize();

        // Send an interactive message with three buttons showcasing the payload/value 
        // being different than the button text
        await client.SendAsync(configuration["SendFrom"]!, new
        {
            messaging_product = "whatsapp",
            recipient_type = "individual",
            to = configuration["SendTo"]!,
            type = "interactive",
            interactive = new
            {
                type = "button",
                body = new
                {
                    text = "Is SpaceX great?"
                },
                action = new
                {
                    buttons = new[]
                    {
                        new { type = "reply", reply = new { id = "btn_yes", title = "Yes" } },
                        new { type = "reply", reply = new { id = "btn_no", title = "No" } },
                    }
                }
            }
        });
    }

    [SecretsFact("Meta:VerifyToken", "SendFrom", "SendTo")]
    public async Task SendsListAsync()
    {
        var (configuration, client) = Initialize();

        var interactive = JsonNode.Parse(
            """
            {
              "action": {
                "button": "Elegir agente",
                "sections": [
                  {
                    "rows": [
                      {
                        "id": "conversation",
                        "title": "Conversación",
                        "description": "Hablar o consultar sobre cualquier tema"
                      },
                      {
                        "id": "tasks",
                        "title": "Tareas",
                        "description": "Gestionar listas y tareas"
                      },
                      {
                        "id": "reminder",
                        "title": "Recordatorios",
                        "description": "Programar recordatorios"
                      },
                      {
                        "id": "order",
                        "title": "Pedidos",
                        "description": "Hacer o gestionar pedidos"
                      }
                    ]
                  }
                ]
              },
              "type": "list",
              "header": {
                "text": "¿Sobre qué tema te gustaría saber qué puedo hacer?",
                "type": "text"
              },
              "body": {
                "text": "Puedo ayudarte con tareas, recordatorios, pedidos, o simplemente conversar. ¡Elegí una opción para continuar!"
              }
            }
            """);

        // Send an interactive message with a JsonNode payload for the interactive node. 
        // showcases using mixed data in the payload for more flexibility.
        await client.SendAsync(configuration["SendFrom"]!, new
        {
            messaging_product = "whatsapp",
            recipient_type = "individual",
            to = configuration["SendTo"]!,
            type = "interactive",
            interactive
        });
    }

    [SecretsFact("Meta:VerifyToken", "SendFrom", "SendTo")]
    public async Task SendsTemplateAsync()
    {
        var (configuration, client) = Initialize();

        await client.SendTemplateAsync(configuration["SendFrom"]!, configuration["SendTo"]!, new MessageTemplate("reminder", "es")
        {
            Body = new BodyComponent([
                new TextParameter("🦷", "emoji"),
                new TextParameter("Dentista", "text"),
                new TextParameter("3pm", "when")
            ])
        });
    }

    [SecretsFact("Meta:VerifyToken", "SendFrom", "SendTo")]
    public async Task SendsTemplateMeetingAsync()
    {
        var (configuration, client) = Initialize();

        await client.SendTemplateAsync(configuration["SendFrom"]!, configuration["SendTo"]!, new MessageTemplate("meeting", "es")
        {
            Header = new HeaderComponent(new LocationParameter(37.483307, -122.148981, "Pablo Morales", "1 Hacker Way, Menlo Park, CA 94025")),
            Body = new BodyComponent(
                [
                    new TextParameter("kzu", "who"),
                    new TextParameter("office", "where"),
                    new TextParameter("15'", "when")
                ])
        });
    }

    [SecretsFact("Meta:VerifyToken", "SendFrom", "SendTo")]
    public async Task SendsTemplate2Async()
    {
        var (configuration, client) = Initialize();

        await client.SendTemplateAsync(configuration["SendFrom"]!, configuration["SendTo"]!, new MessageTemplate("reminder2", "en")
        {
            Header = new HeaderComponent(new LocationParameter(37.483307, -122.148981, "Pablo Morales", "1 Hacker Way, Menlo Park, CA 94025")),
            Body = new BodyComponent(
            [
                new TextParameter("🦷", "emoji"),
                new TextParameter("Dentista", "text"),
                new TextParameter("3pm", "when")
            ])
        });
    }

    [SecretsFact("Meta:VerifyToken", "SendFrom", "SendTo")]
    public async Task SendsTemplateUrlAsync()
    {
        var (configuration, client) = Initialize();

        await client.SendTemplateAsync(configuration["SendFrom"]!, configuration["SendTo"]!, new MessageTemplate("variables", "en")
        {
            Header = new HeaderComponent(new TextParameter("kzu", "name")),
            Body = new BodyComponent(
            [
                new TextParameter("dotnet", "tag"),
            ]),
            Buttons =
            [
                ButtonComponent.Url("dotnet"),
                ButtonComponent.Default
            ]
        });
    }

    [SecretsFact("Meta:VerifyToken", "SendFrom", "SendTo")]
    public async Task SendsTemplateButtonsAsync()
    {
        var (configuration, client) = Initialize();

        await client.SendTemplateAsync(configuration["SendFrom"]!, configuration["SendTo"]!, new MessageTemplate("buttons", "en")
        {
            Buttons =
            [
                ButtonComponent.Payload("id1"),
                //NOTE: we can omit the buttons if we don't need custom payloads for them. 
                // the webhook will get the payload == button text in that case.
                //ButtonComponent.Text("id2"),
                // Since we omitted the second button, we'll need to specify the index in this case.
                // otherwise, it defaults to its index in the array.
                ButtonComponent.Url("dotnet", index: 2),
            ]
        });
    }

    [SecretsFact("Meta:VerifyToken", "SendFrom", "SendTo")]
    public async Task SendsCallToActionAsync()
    {
        var (configuration, client) = Initialize();

        // Send an interactive message with three buttons showcasing the payload/value 
        // being different than the button text
        await client.SendAsync(configuration["SendFrom"]!, new
        {
            messaging_product = "whatsapp",
            recipient_type = "individual",
            to = configuration["SendTo"]!,
            type = "interactive",
            interactive = new
            {
                type = "cta_url",
                body = new
                {
                    text = "Tap the button to send a message to a contact"
                },
                action = new
                {
                    name = "cta_url",
                    parameters = new
                    {
                        display_text = "Send",
                        url = "https://wa.me/541234567890?text=Hi"
                    }
                }
            }
        });
    }

    [SecretsFact("Meta:VerifyToken", "MediaTo", Skip = "Media attachments are deleted if user deletes them, so skip.")]
    public async Task ResolvesMediaIdFromHttpClient()
    {
        var (configuration, client) = Initialize();

        var media = await client.ResolveMediaAsync(configuration["MediaTo"]!, "734245612619207");

        Assert.NotNull(media);

        using var http = client.CreateHttp(configuration["MediaTo"]!);
        var stream = await http.GetStreamAsync(media.Url);
        using var fs = new FileStream("document.pdf", FileMode.Create, FileAccess.Write);
        await stream.CopyToAsync(fs);
    }

    [SecretsFact("Meta:VerifyToken", "MediaTo", Skip = "Media are transient and therefore this test requires an active message present")]
    public async Task ResolveMediaThrowsForNonExistentId()
    {
        var (configuration, client) = Initialize();

        var ex = await Assert.ThrowsAsync<GraphMethodException>(() => client.ResolveMediaAsync(configuration["MediaTo"]!, "123456789"));

        Assert.Contains("123456789", ex.Message);
        Assert.Equal(100, ex.Code);
        Assert.Equal(33, ex.Subcode);
    }

    [SecretsFact("Meta:VerifyToken", "MediaTo")]
    public async Task ResolveMediaThrowsForNonMediaMessage()
    {
        var (configuration, client) = Initialize();

        await Assert.ThrowsAsync<NotSupportedException>(() => client.ResolveMediaAsync(
            new ContentMessage("asdf", new Service("asdf", "1234"), new User("kzu", "2134"), 0,
                new UnknownContent(new System.Text.Json.JsonElement()))));
    }

    [SecretsFact("Meta:VerifyToken", "SendFrom", "SendTo")]
    public async Task SendsTemplateWithMessageTemplateObjectAsync()
    {
        var (configuration, client) = Initialize();

        // Using the new MessageTemplate object instead of anonymous object
        var template = new MessageTemplate("reminder", "es")
        {
            Body = new BodyComponent([
                new TextParameter("🦷", "emoji"),
                new TextParameter("Dentista", "text"),
                new TextParameter("3pm", "when")
            ])
        };

        await client.SendTemplateAsync(configuration["SendFrom"]!, configuration["SendTo"]!, template);
    }

    record Media(string Url, string MimeType, long FileSize);

    (IConfiguration configuration, WhatsAppClient client) Initialize()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<WhatsAppClientTests>()
            .Build();

        var collection = new ServiceCollection()
            .AddSingleton<ILoggerFactory>(new MockLogger(output))
            .AddHttpClient()
            .AddSingleton<IConfiguration>(configuration);

        collection.AddOptions<MetaOptions>()
            .BindConfiguration("Meta")
            .ValidateDataAnnotations();

        collection.AddSingleton<WhatsAppClient>();

        var services = collection.BuildServiceProvider();
        return (configuration, services.GetRequiredService<WhatsAppClient>());
    }
}
