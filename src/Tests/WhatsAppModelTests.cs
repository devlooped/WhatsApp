using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Devlooped;
using Xunit;
using Xunit.Abstractions;

namespace Devlooped.WhatsApp;

public class WhatsAppModelTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData(nameof(ContentType.Audio), "927483105672819", "wamid.XYZRandomString123ABC456DEF789GHI==")]
    [InlineData(nameof(ContentType.Contact), "927481035162874", "wamid.HBgNNDcyODkwMTIzNDU2NhUCABIYFjE4QTlDMzU2MkJDOTg3RUY2NDg5RTFEMTIzQzVFRAA==")]
    [InlineData(nameof(ContentType.Document), "813947205126374", "wamid.HBgNMTIwMjU1NTk4NzY1NhUCABIYFjE4QTlDMzU2MkJDOTg3RUY2NDg5RTFEMTIzQzVFRAA==")]
    [InlineData(nameof(ContentType.Image), "813927405162784", "wamid.HBgNMTIwMjU1NTk4NzY1NhUCABIYFjE4QTlDMzU2MkJDOTg3RUY2NDg5RTFEMTIzQzVFRAA==")]
    [InlineData(nameof(ContentType.Location), "813920475601234", "wamid.HBgNMTIwMjk4NzQ1NjM1NhUCABIYFjE5RDhGMzQ2NEJDOTg3RUY2NDg5RTFEMTIzQzVFRAA==")]
    [InlineData(nameof(ContentType.Text), "813920475102346", "wamid.HBgNMTIwMjk4NzQ1NjM1NhUCABIYFjQ5RjE4QzJEMzU2ODk3QTJFMUY3RDEyMjNBNkI5QwA==", "wamid.HBgNNTQ5MTE1OTL4ODI4MhUCBBEYEjUxNDI3NkMzRkI1ODVCRTgwOAA=")]
    [InlineData(nameof(ContentType.Video), "813927405162374", "wamid.HBgNMTIwMjU1NTk4NzY1NhUCABIYFjE4QTlDMzU2MkJDOTg3RUY2NDg5RTFEMTIzQzVFRAA==")]
    [InlineData(nameof(MessageType.Unsupported), "837625914708254", "wamid.HBgNNTQ5MzcyNjEwNDg1OVUCABIYFjJCRDM5RTg0QkY3OEQxMjM2RkE0QjcA")]
    [InlineData(nameof(MessageType.Error), "729104583621947", "wamid.XYZgMDEyMzQ1Njc4OTA5MRUCABEYEjU5NkM3ODlFQjAxMjM0NTY7OA==")]
    [InlineData(nameof(MessageType.Interactive), "123456789012345", "wamid.RandomMessageID", "wamid.RandomContextID")]
    [InlineData(nameof(MessageType.Reaction), "123456789012345", "wamid.HBgNMTIzNDU2Nzg5MDEyMzQ1MhUCABEYEkY5QzQxNDNBQjgyRkVENEIzMQA=", "wamid.HBgNMTIzNDU2Nzg5MDEyMzQ1MhUCABEYEkY5QzQxNDNBQjgyRkVENEIzMQA=")]
    // For consistency, status message ID == status context ID.
    [InlineData(nameof(MessageType.Status), "987654321098765", "wamid.HBgNNTQ5OTg3NjU0MzIxMDlUCABEYEkLMNVzNDU2Nzg5MAA=", "wamid.HBgNNTQ5OTg3NjU0MzIxMDlUCABEYEkLMNVzNDU2Nzg5MAA=")]
    public async Task DeserializeMessage(string type, string notification, string id, string? context = default)
    {
        var json = await File.ReadAllTextAsync($"Content/WhatsApp/{type}.json");
        var message = await Message.DeserializeAsync(json);

        Assert.NotNull(message);
        Assert.Equal(notification, message.NotificationId);
        Assert.Equal(id, message.Id);
        Assert.Equal(context, message.Context);
        Assert.NotNull(message.To);
        Assert.NotNull(message.From);
    }

    [Theory]
    [InlineData(ContentType.Audio)]
    [InlineData(ContentType.Contact)]
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
        Assert.NotNull(message.To);
        Assert.NotNull(message.From);
        Assert.NotNull(content.Content);
        Assert.Equal(type, content.Content.Type);
    }

    [Fact]
    public async Task DeserializeErrorStatus()
    {
        var json = await File.ReadAllTextAsync($"Content/WhatsApp/Error.json");
        var message = await Message.DeserializeAsync(json);

        var error = Assert.IsType<ErrorMessage>(message);

        Assert.NotNull(message);
        Assert.NotNull(message.NotificationId);
        Assert.NotNull(message.To);
        Assert.NotNull(message.From);
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
        Assert.NotNull(message.To);
        Assert.NotNull(message.From);
        Assert.Equal(Status.Delivered, status.Status);
    }

    [Fact]
    public async Task DeserializeInteractive()
    {
        var json = await File.ReadAllTextAsync($"Content/WhatsApp/Interactive.json");
        var message = await Message.DeserializeAsync(json);

        var interactive = Assert.IsType<InteractiveMessage>(message);

        Assert.NotNull(message);
        Assert.NotNull(message.NotificationId);
        Assert.NotNull(message.To);
        Assert.NotNull(message.From);
        Assert.Equal("btn_yes", interactive.Button.Id);
        Assert.Equal("Yes", interactive.Button.Title);
    }

    [Fact]
    public async Task DeserializeUnsupported()
    {
        var json = await File.ReadAllTextAsync($"Content/WhatsApp/Unsupported.json");
        var message = await Message.DeserializeAsync(json);

        var unsupported = Assert.IsType<UnsupportedMessage>(message);

        Assert.NotNull(message);
        Assert.NotNull(message.NotificationId);
        Assert.NotNull(message.To);
        Assert.NotNull(message.From);
    }

    [Fact]
    public async Task DeserializeReaction()
    {
        var json = await File.ReadAllTextAsync($"Content/WhatsApp/Reaction.json");
        var message = await Message.DeserializeAsync(json);

        var reaction = Assert.IsType<ReactionMessage>(message);

        Assert.NotNull(message);
        Assert.NotNull(message.NotificationId);
        Assert.NotNull(message.To);
        Assert.NotNull(message.From);
        Assert.Equal("😊", reaction.Emoji);
    }
}
