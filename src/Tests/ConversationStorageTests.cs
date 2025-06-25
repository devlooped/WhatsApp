namespace Devlooped.WhatsApp;

public class ConversationStorageTests
{
    readonly static Service service = new("1234", "1234");
    readonly static User user = new("kzu", "5678");

    [Fact]
    public async Task StoreAndLoadAdditionalProperties()
    {
        var storage = new ConversationStorage(CloudStorageAccount.DevelopmentStorageAccount);
        var messageId = Ulid.NewUlid().ToString();
        var conversationId = Ulid.NewUlid().ToString();

        await storage.SaveAsync(new ContentMessage(
            messageId,
            service, user, DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            new TextContent("Hello")
            {
                AdditionalProperties = new()
                {
                    { "ContentProp", "ContentValue" },
                    { "ContentNum", 42 },
                    { "ContentBool", true },
                }
            })
        {
            ConversationId = conversationId,
            AdditionalProperties = new()
            {
                { "MessageProp", "MessageValue" },
                { "MessageNum", 42 },
                { "MessageBool", true },
            }
        });

        var message = await storage.GetMessageAsync(user.Number, messageId);

        Assert.NotNull(message);
        // Assert AdditionalProperties on message and content
        var content = Assert.IsType<ContentMessage>(message);
        Assert.NotNull(content.AdditionalProperties);
        Assert.Equal("MessageValue", (string)content.AdditionalProperties["MessageProp"]!);
        Assert.Equal(42, content.AdditionalProperties["MessageNum"]);
        Assert.True((bool)content.AdditionalProperties["MessageBool"]!);

        var text = Assert.IsType<TextContent>(content.Content);
        Assert.NotNull(text.AdditionalProperties);
        Assert.Equal("ContentValue", (string)text.AdditionalProperties["ContentProp"]!);
        Assert.Equal(42, text.AdditionalProperties["ContentNum"]);
        Assert.True((bool)text.AdditionalProperties["ContentBool"]!);

        await foreach (var entry in storage.GetMessagesAsync(user.Number, conversationId))
        {
            content = Assert.IsType<ContentMessage>(message);
            Assert.NotNull(content.AdditionalProperties);
            Assert.Equal("MessageValue", (string)content.AdditionalProperties["MessageProp"]!);

            text = Assert.IsType<TextContent>(content.Content);
            Assert.NotNull(text.AdditionalProperties);
            Assert.Equal("ContentValue", (string)text.AdditionalProperties["ContentProp"]!);
        }

        message.AdditionalProperties?["Agent"] = "Calendar";

        await storage.SaveAsync(message);

        var updatedMessage = await storage.GetMessageAsync(user.Number, messageId);

        Assert.NotNull(updatedMessage?.AdditionalProperties);
        Assert.Equal("Calendar", updatedMessage.AdditionalProperties?["Agent"]);
        Assert.Equal("MessageValue", (string)updatedMessage.AdditionalProperties!["MessageProp"]!);
    }
}
