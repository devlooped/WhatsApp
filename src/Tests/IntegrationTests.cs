using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Devlooped.WhatsApp;

public class IntegrationTests : IDisposable
{
    readonly static Service service = new("1234", "1234");
    readonly static User user = new("kzu", "5678");

    public void Dispose()
    {
        var client = CloudStorageAccount.DevelopmentStorageAccount.CreateTableServiceClient();
        try
        {
            client.DeleteTable("WhatsAppMessages");
        }
        catch { }
        try
        {
            client.DeleteTable("WhatsAppConversations");
        }
        catch { }
    }

    [Fact]
    public async Task RunConversationAsync()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()
            {
                { "Meta:VerifyToken", "test-challenge" },
                { "Meta:Numbers:1234567890", "test-access-token" }
            })
            .Build();

        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton<IConversationStorage>(new TestConversationStorage(CloudStorageAccount.DevelopmentStorageAccount));

        IEnumerable<IMessage>? messages = null;

        services
            .AddWhatsApp((input, cancellation) =>
            {
                messages = input.ToArray();
                return AsyncEnumerable.Empty<Response>();
            })
            .UseConversation();

        var provider = services.BuildServiceProvider();
        var handler = provider.GetRequiredService<IWhatsAppHandler>();

        await handler.HandleAsync([
            new ContentMessage(Ulid.NewUlid().ToString(), service, user, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                new TextContent("Hello"))])
            .ToListAsync();

        Assert.NotNull(messages);
        Assert.Single(messages);

        await handler.HandleAsync([
            new ContentMessage(Ulid.NewUlid().ToString(), service, user, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                new TextContent("Bye"))])
            .ToListAsync();

        // Conversation storage kicks in and populates the previous message too.
        Assert.NotNull(messages);
        Assert.Equal(2, messages.Count());
    }
}
