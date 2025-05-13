using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

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
            new ContentMessage(id,
                new Service(from, from),
                new User(to, to),
                DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                new TextContent("Hi there!")),
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

    [SecretsFact("Meta:VerifyToken", "MediaTo")]
    public async Task ResolvesMediaIdFromHttpClient()
    {
        var (configuration, client) = Initialize();

        var media = await client.ResolveMediaAsync(configuration["MediaTo"]!, "4075001832719300");

        Assert.NotNull(media);

        using var http = client.CreateHttp(configuration["MediaTo"]!);
        var stream = await http.GetStreamAsync(media.Url);
        using var fs = new FileStream("document.pdf", FileMode.Create, FileAccess.Write);
        await stream.CopyToAsync(fs);
    }

    [SecretsFact("Meta:VerifyToken", "MediaTo")]
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
