using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Devlooped.WhatsApp;

public class PipelineTests(ITestOutputHelper output)
{
    readonly static Service service = new("1234", "1234");
    readonly static User user = new("kzu", "5678");

    [Fact]
    public async Task CanBuildEmptyPipeline()
    {
        var builder = new WhatsAppHandlerBuilder();

        var handler = builder.Build();

        await handler.HandleAsync(new ReactionMessage("1234", service, user, 0, "🗽"));
    }

    [Fact]
    public async Task CanBuildDecoratingPipeline()
    {
        var called = false;

        var pipeline = new WhatsAppHandlerBuilder()
            .Use((message, inner, cancellation) =>
            {
                called = true;
                return inner.HandleAsync(message, cancellation);
            })
            .Use(handler => WhatsAppHandler.Continue)
            .Build();

        await pipeline.HandleAsync(new ReactionMessage("1234", service, user, 0, "🗽"));

        Assert.True(called);
    }

    [Fact]
    public async Task CanBuildLoggingPipeline()
    {
        var after = false;
        var before = false;
        var target = true;

        var pipeline = new WhatsAppHandlerBuilder(
            services => AnonymousWhatsAppHandler.Create(services, (messages, cancellation) =>
            {
                Assert.True(before);
                Assert.True(target);
                target = true;

                return AsyncEnumerable.Empty<Response>();
            }))
            .Use((message, inner, cancellation) =>
            {
                before = true;
                Assert.False(after);
                return inner.HandleAsync(message, cancellation);
            })
            .UseLogging(output.AsLoggerFactory())
            .Use((message, inner, cancellation) =>
            {
                Assert.True(before);
                after = true;
                return inner.HandleAsync(message, cancellation);
            })
            .Build();

        await pipeline.HandleAsync(new ReactionMessage("1234", service, user, 0, "🗽"));

        Assert.True(before);
        Assert.True(after);
        Assert.True(target);
    }

    [Fact]
    public async Task ConversationCalledAfterCustom()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()
            {
                { "Meta:VerifyToken", "test-challenge" },
                { "Meta:Numbers:1234567890", "test-access-token" }
            })
            .Build();

        IAsyncEnumerable<Response> messages = new Response[]
        {
            new TextResponse("123", "456", "Foo", "Bar")
            {
                ConversationId = "Baz"
            }
        }.ToAsyncEnumerable();

        var order = new List<string>();

        var handler = Mock.Of<IWhatsAppHandler>(x => x.HandleAsync(It.IsAny<IEnumerable<IMessage>>(), It.IsAny<CancellationToken>()) == messages);
        var conversation = new Mock<IConversationStorage>();

        conversation
            .Setup(x => x.GetMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback(() => order.Add("storage:read"));

        conversation
            .Setup(x => x.GetMessagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns<string, string, CancellationToken>((number, conversation, cancellation) =>
            {
                order.Add("storage:all");
                return messages;
            });

        conversation
            .Setup(x => x.GetActiveConversationAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback(() => order.Add("storage:active"));

        conversation
            .Setup(x => x.SaveAsync(It.IsAny<IMessage>(), It.IsAny<CancellationToken>()))
            .Callback(() => order.Add("storage:save"));

        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton(conversation.Object);

        services.AddWhatsApp(handler)
            .Use((messages, inner, cancellation) =>
            {
                order.Add("before");
                return inner.HandleAsync(messages, cancellation);
            })
            .UseConversation()
            .Use((messages, inner, cancellation) =>
            {
                order.Add("after");
                return inner.HandleAsync(messages, cancellation);
            });

        // Override default IWhatsAppClient to prevent any actual sending
        var client = new Mock<IWhatsAppClient>();
        client.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
              .Callback(() => order.Add("client:send"));

        services.AddSingleton<IWhatsAppClient>(client.Object);

        var provider = services.BuildServiceProvider();
        await provider.GetRequiredService<IWhatsAppHandler>()
            .HandleAsync(new ReactionMessage("msgid", service, user, 0, "🗽"));

        //             custom    👇 get convo id   👇 input 💾   👇 convo 🔎    custom   👇 output 📨  👇 response 💾
        Assert.Equal(["before", "storage:active", "storage:save", "storage:all", "after", "client:send", "storage:save"], order);
    }

    [Fact]
    public async Task ConversationRestored()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()
            {
                { "Meta:VerifyToken", "test-challenge" },
                { "Meta:Numbers:1234567890", "test-access-token" }
            })
            .Build();

        var storage = new MemoryConversationStorage();
        var response = AsyncEnum<Response>([new TextResponse(service.Id, user.Number, "1234", null)
        {
            ConversationId = "Bye"
        }]);
        var messages = new List<IMessage[]>();

        var handler = new Mock<IWhatsAppHandler>();
        handler.Setup(x => x.HandleAsync(It.IsAny<IEnumerable<IMessage>>(), It.IsAny<CancellationToken>()))
            .Returns((IEnumerable<IMessage> input, CancellationToken _) =>
            {
                messages.Add(input.ToArray());
                // Timestamp in WhatsApp is in Unix seconds, so we need to simulate a delay
                Thread.Sleep(1000);
                var message = input.OfType<ContentMessage>().Last();
                return AsyncEnum<Response>([message.Reply(message.Content.ToString() + " Reply")]);
            });

        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton<IConversationStorage>(storage);

        services.AddWhatsApp(handler.Object).UseConversation();

        // Override default IWhatsAppClient to prevent any actual sending
        var client = new Mock<IWhatsAppClient>();
        client.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(Ulid.NewUlid().ToString());

        services.AddSingleton<IWhatsAppClient>(client.Object);

        var provider = services.BuildServiceProvider();
        var pipeline = provider.GetRequiredService<IWhatsAppHandler>();

        await pipeline.HandleAsync(Text("Hello"));

        Assert.Single(messages);
        Assert.Single(messages[0]);

        // Timestamps in WhatsApp are in Unix seconds, so we need to wait.
        Thread.Sleep(1000);
        await pipeline.HandleAsync(Text("Hello Again"));

        Assert.Equal(2, messages.Count);
        // Second messages now contain first message, its response and the new one
        Assert.Equal(3, messages[1].Length);
        Assert.IsAssignableFrom<ContentMessage>(messages[1][0]);
        Assert.IsAssignableFrom<Response>(messages[1][1]);
        Assert.IsAssignableFrom<ContentMessage>(messages[1][2]);
    }

    [Fact]
    public async Task CanSendMessagesThroughPipeline()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()
            {
                { "Meta:VerifyToken", "test-challenge" },
                { "Meta:Numbers:1234", "test-access-token" }
            })
            .Build();

        var handler = new Mock<IWhatsAppHandler>();
        handler.Setup(x => x.HandleAsync(It.IsAny<IEnumerable<IMessage>>(), It.IsAny<CancellationToken>()))
            .Returns((IEnumerable<IMessage> input, CancellationToken _) =>
            {
                // Timestamp in WhatsApp is in Unix seconds, so we need to simulate a delay
                Thread.Sleep(1000);
                var message = input.OfType<ContentMessage>().Last();
                return AsyncEnum<Response>([message.Reply(message.Content.ToString() + " Reply")]);
            });

        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration);

        var sent = 0;

        // Override default IWhatsAppClient to prevent any actual sending
        var client = new Mock<IWhatsAppClient>();
        client.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
              .Callback(() => sent++)
              .ReturnsAsync(Ulid.NewUlid().ToString());

        services.AddSingleton<IWhatsAppClient>(client.Object);

        services.AddWhatsApp(handler.Object)
            .Use(EchoAndHandle);

        var pipeline = services.BuildServiceProvider().GetRequiredService<IWhatsAppHandler>();
        await pipeline.HandleAsync(Text("Hello"));

        // One from the echo, one from the actualy reply.
        Assert.Equal(2, sent);

        await pipeline.HandleAsync(Text("Again"));

        Assert.Equal(4, sent);
    }

    static async IAsyncEnumerable<Response> EchoAndHandle(IEnumerable<IMessage> messages, IWhatsAppHandler inner, [EnumeratorCancellation] CancellationToken cancellation)
    {
        var content = messages.OfType<ContentMessage>().LastOrDefault();
        if (content != null)
            yield return content.Reply("Echo: " + content.Content.ToString());

        await foreach (var response in inner.HandleAsync(messages, cancellation))
            yield return response;
    }

    ContentMessage Text(string text) => new ContentMessage(
        Ulid.NewUlid().ToString(), service, user, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), new TextContent(text));

    IAsyncEnumerable<IMessage> AsyncEnum(IEnumerable<IMessage> messages) => messages.ToAsyncEnumerable();

    IAsyncEnumerable<T> AsyncEnum<T>(IEnumerable<T> messages) => messages.ToAsyncEnumerable();

    class MemoryConversationStorage : ConversationStorage
    {
        public MemoryConversationStorage() : base(CloudStorageAccount.DevelopmentStorageAccount) { }

        protected override IDocumentRepository<Conversation> CreateActiveConversationRepository()
            => MemoryRepository.Create<Conversation>(rowKey: _ => "active");

        protected override IDocumentRepository<Conversation> CreateConversationsRepository()
            => MemoryRepository.Create<Conversation>();

        protected override IDocumentRepository<IMessage> CreateMessagesRepository()
            => MemoryRepository.Create<IMessage>("messages", x => x.UserNumber, x => x.Id);
    }
}
