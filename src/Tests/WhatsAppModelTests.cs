using System.Text.Json;

namespace Devlooped.WhatsApp;

public class WhatsAppModelTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData(nameof(ContentType.Audio), "927483105672819", "wamid.XYZRandomString123ABC456DEF789GHI==")]
    [InlineData(nameof(ContentType.Contact), "927481035162874", "wamid.HBgNNDcyODkwMTIzNDU2NhUCABIYFjE4QTlDMzU2MkJDOTg3RUY2NDg5RTFEMTIzQzVFRAA==")]
    [InlineData(nameof(ContentType.Contacts), "123456789012345", "wamid.ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890")]
    [InlineData(nameof(ContentType.Document), "813947205126374", "wamid.HBgNMTIwMjU1NTk4NzY1NhUCABIYFjE4QTlDMzU2MkJDOTg3RUY2NDg5RTFEMTIzQzVFRAA==")]
    [InlineData(nameof(ContentType.Image), "813927405162784", "wamid.HBgNMTIwMjU1NTk4NzY1NhUCABIYFjE4QTlDMzU2MkJDOTg3RUY2NDg5RTFEMTIzQzVFRAA==")]
    [InlineData(nameof(ContentType.Location), "813920475601234", "wamid.HBgNMTIwMjk4NzQ1NjM1NhUCABIYFjE5RDhGMzQ2NEJDOTg3RUY2NDg5RTFEMTIzQzVFRAA==")]
    [InlineData(nameof(ContentType.Text), "813920475102346", "wamid.HBgNMTIwMjk4NzQ1NjM1NhUCABIYFjQ5RjE4QzJEMzU2ODk3QTJFMUY3RDEyMjNBNkI5QwA==", "wamid.HBgNNTQ5MTE1OTL4ODI4MhUCBBEYEjUxNDI3NkMzRkI1ODVCRTgwOAA=")]
    [InlineData(nameof(ContentType.Video), "813927405162374", "wamid.HBgNMTIwMjU1NTk4NzY1NhUCABIYFjE4QTlDMzU2MkJDOTg3RUY2NDg5RTFEMTIzQzVFRAA==")]
    [InlineData(nameof(MessageType.Unsupported), "837625914708254", "")]
    [InlineData(nameof(MessageType.Error), "729104583621947", "")]
    [InlineData(nameof(MessageType.Reaction), "123456789012345", "", "wamid.HBgNMTIzNDU2Nzg5MDEyMzQ1MhUCABEYEkY5QzQxNDNBQjgyRkVENEIzMQA=")]
    // For consistency, status message ID == status context ID.
    [InlineData(nameof(MessageType.Status), "987654321098765", "", "wamid.HBgNNTQ5OTg3NjU0MzIxMDlUCABEYEkLMNVzNDU2Nzg5MAA=")]
    public async Task DeserializeMessage(string type, string notification, string id, string? context = default)
    {
        var json = await File.ReadAllTextAsync($"Content/WhatsApp/{type}.json");
        var message = await Message.DeserializeAsync(json);

        Assert.NotNull(message);
        Assert.Equal(notification, message.NotificationId);
        // If id was empty, it should be automatically fixed and generated
        if (id == string.Empty)
        {
            Assert.False(string.IsNullOrEmpty(message.Id));
        }
        else
        {
            Assert.Equal(id, message.Id);
        }
        Assert.Equal(context, message.Context);
        Assert.NotNull(message.Service);
        Assert.NotNull(message.User);
    }

    [Theory]
    [InlineData(ContentType.Audio)]
    [InlineData(ContentType.Contacts)]
    [InlineData(ContentType.Document)]
    [InlineData(ContentType.Image)]
    [InlineData(ContentType.Location)]
    [InlineData(ContentType.Text)]
    [InlineData(ContentType.Video)]
    public async Task DeserializeContent(ContentType type)
    {
        var json = await File.ReadAllTextAsync($"Content/WhatsApp/{type}.json");
        var message = await Message.DeserializeAsync(json);

        var content = Assert.IsType<ContentMessage>(message);

        Assert.NotNull(message);
        Assert.NotNull(message.NotificationId);
        Assert.NotNull(message.Service);
        Assert.NotNull(message.User);
        Assert.NotNull(content.Content);
        Assert.Equal(type, content.Content.Type);
    }

    [Fact]
    public async Task DeserializeContacts()
    {
        var json = await File.ReadAllTextAsync($"Content/WhatsApp/Contacts.json");
        var message = await Message.DeserializeAsync(json);

        var content = Assert.IsType<ContentMessage>(message);

        Assert.Equal(ContentType.Contacts, content.Content.Type);
        var contacts = Assert.IsType<ContactsContent>(content.Content);
        Assert.Equal(3, contacts.Contacts.Length);

        // Assert for first contact
        Assert.Equal("First1", contacts.Contacts[0].Name);
        Assert.Equal("1111111111111", contacts.Contacts[0].Numbers[0]);

        // Assert for second contact
        Assert.Equal("First2", contacts.Contacts[1].Name);
        Assert.Equal("2222222222222", contacts.Contacts[1].Numbers[0]);

        // Assert for third contact
        Assert.Equal("First3", contacts.Contacts[2].Name);
        Assert.Equal("3333333333333", contacts.Contacts[2].Numbers[0]);
    }

    [Fact]
    public async Task DeserializeErrorStatus()
    {
        var json = await File.ReadAllTextAsync($"Content/WhatsApp/Error.json");
        var message = await Message.DeserializeAsync(json);

        var error = Assert.IsType<ErrorMessage>(message);

        Assert.NotNull(message);
        Assert.NotNull(message.NotificationId);
        Assert.NotNull(message.Service);
        Assert.NotNull(message.User);
        Assert.NotNull(error.Error);
        Assert.Equal(470, error.Error.Code);
    }

    [Fact]
    public async Task DeserializeStatus()
    {
        var json = await File.ReadAllTextAsync($"Content/WhatsApp/Status.json");
        var message = await Message.DeserializeAsync(json);

        var status = Assert.IsType<StatusMessage>(message);

        Assert.NotNull(message);
        Assert.NotNull(message.NotificationId);
        Assert.NotNull(message.Service);
        Assert.NotNull(message.User);
        Assert.Equal(Status.Delivered, status.Status);
    }

    [Fact]
    public async Task DeserializeInteractiveButton()
    {
        var json = await File.ReadAllTextAsync($"Content/WhatsApp/InteractiveButton.json");
        var message = await Message.DeserializeAsync(json);

        var interactive = Assert.IsType<InteractiveMessage>(message);

        Assert.NotNull(message);
        Assert.NotNull(message.NotificationId);
        Assert.NotNull(message.Service);
        Assert.NotNull(message.User);
        Assert.Equal("btn_yes", interactive.Selection.Id);
        Assert.Equal("Yes", interactive.Selection.Title);
    }

    [Fact]
    public async Task DeserializeInteractiveList()
    {
        var json = await File.ReadAllTextAsync($"Content/WhatsApp/InteractiveList.json");
        var message = await Message.DeserializeAsync(json);

        var interactive = Assert.IsType<InteractiveMessage>(message);

        Assert.NotNull(message);
        Assert.NotNull(message.NotificationId);
        Assert.NotNull(message.Service);
        Assert.NotNull(message.User);
        Assert.Equal("conversation", interactive.Selection.Id);
        Assert.Equal("Conversación", interactive.Selection.Title);
    }

    [Fact]
    public async Task DeserializeUnsupported()
    {
        var json = await File.ReadAllTextAsync($"Content/WhatsApp/Unsupported.json");
        var message = await Message.DeserializeAsync(json);

        var unsupported = Assert.IsType<UnsupportedMessage>(message);

        Assert.NotNull(message);
        Assert.NotNull(message.NotificationId);
        Assert.NotNull(message.Service);
        Assert.NotNull(message.User);
    }

    [Fact]
    public async Task DeserializeReaction()
    {
        var json = await File.ReadAllTextAsync($"Content/WhatsApp/Reaction.json");
        var message = await Message.DeserializeAsync(json);

        var reaction = Assert.IsType<ReactionMessage>(message);

        Assert.NotNull(message);
        Assert.NotNull(message.NotificationId);
        Assert.NotNull(message.Service);
        Assert.NotNull(message.User);
        Assert.Equal("😊", reaction.Emoji);
    }

    [Fact]
    public void SerializeAnonymous()
    {
        var response = Response.Create("123456789012345", "987654321098765", (client, cancellation) => Task.FromResult<string?>(null));

        var json = JsonSerializer.Serialize(response, JsonContext.DefaultOptions);

        var message = JsonSerializer.Deserialize(json, JsonContext.Default.AnonymousResponse);

        Assert.NotNull(message);
        Assert.Null(message.Sender);
    }
}
