using System;
using System.Collections.Generic;
using System.Linq;
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
    [InlineData(
        """
        {
          "object": "whatsapp_business_account",
          "entry": [
            {
              "id": "554372691093163",
              "changes": [
                {
                  "value": {
                    "messaging_product": "whatsapp",
                    "metadata": {
                      "display_phone_number": "5491123960774",
                      "phone_number_id": "524718744066632"
                    },
                    "contacts": [
                      {
                        "profile": { "name": "Kzu" },
                        "wa_id": "5491159278282"
                      }
                    ],
                    "messages": [
                      {
                        "from": "5491159278282",
                        "id": "wamid.HBgNNTQ5MTE1OTI3ODI4MhUCABIYFjNFQjBFOEFGODAzMEI4RTI3NzczNjkA",
                        "timestamp": "1744062742",
                        "text": { "body": "hello!" },
                        "type": "text"
                      }
                    ]
                  },
                  "field": "messages"
                }
              ]
            }
          ]
        }
        """)]
    public async Task DeserializePayload(string json)
    {
        var message = await Message.DeserializeAsync(json);
        Assert.NotNull(message);
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

    [Fact]
    public async Task DeserializeStatus()
    {
        var json = await File.ReadAllTextAsync($"Content/WhatsApp/Status.json");
        var message = await Message.DeserializeAsync(json);

        var status = Assert.IsType<StatusMessage>(message);

        Assert.NotNull(message);
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
        Assert.NotNull(message.To);
        Assert.NotNull(message.From);
        Assert.Equal("btn_yes", interactive.Button.Id);
        Assert.Equal("Yes", interactive.Button.Title);
    }
}
