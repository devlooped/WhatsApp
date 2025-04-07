using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Devlooped;
using Xunit;

namespace Devlooped.WhatsApp;

public class WhatsAppTests
{
    [Theory]
    [InlineData(ContentType.Audio)]
    [InlineData(ContentType.Contact)]
    [InlineData(ContentType.Document)]
    [InlineData(ContentType.Image)]
    [InlineData(ContentType.Location)]
    [InlineData(ContentType.Text)]
    [InlineData(ContentType.Video)]
    public async Task DeserializePolymorphic(ContentType type)
    {
        var json = await File.ReadAllTextAsync($"Content/WhatsApp/{type}.json");
        var message = await Message.DeserializeAsync(json);

        var content = Assert.IsType<ContentMessage>(message);

        Assert.NotNull(message);
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
        Assert.NotNull(message.To);
        Assert.NotNull(message.From);
        Assert.NotNull(error.Error);
        Assert.Equal(470, error.Error.Code);
    }
}
