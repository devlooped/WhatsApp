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
                return Task.CompletedTask;
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
}
