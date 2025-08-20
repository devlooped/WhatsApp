using System.Text.Json;
using Moq;

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
    public async Task DeserializeTemplateButton()
    {
        var json = await File.ReadAllTextAsync($"Content/WhatsApp/TemplateButton.json");
        var message = await Message.DeserializeAsync(json);

        var interactive = Assert.IsType<InteractiveMessage>(message);

        Assert.NotNull(message);
        Assert.NotNull(message.NotificationId);
        Assert.NotNull(message.Service);
        Assert.NotNull(message.User);
        Assert.Equal("id1", interactive.Selection.Id);
        Assert.Equal("Track", interactive.Selection.Title);
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
    public async Task DeserializeInteractiveFlow()
    {
        var json = await File.ReadAllTextAsync($"Content/WhatsApp/InteractiveFlow.json");
        var message = await Message.DeserializeAsync(json);

        var interactive = Assert.IsType<InteractiveFlowMessage>(message);

        Assert.NotNull(message);
        Assert.NotNull(message.NotificationId);
        Assert.NotNull(message.Service);
        Assert.NotNull(message.User);
        Assert.Equal("Hola", interactive.Data.GetProperty("comment").GetString());
        Assert.NotNull(interactive.Source);
        Assert.Equal("data", interactive.Source.Flow);
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
    public async Task SerializeAnonymous()
    {
        var response = Response.Create("123456789012345", "987654321098765", (client, cancellation) => Task.FromResult<string?>("asdf"));

        var json = JsonSerializer.Serialize(response, JsonContext.DefaultOptions);

        var message = JsonSerializer.Deserialize(json, JsonContext.Default.AnonymousResponse);

        Assert.NotNull(message);

        // the value is either null due to not being deserialized, or it's a dummy function that returns null
        // since that's the default impl. in the [JsonConstructor]
        if (message.Sender != null)
            Assert.Null(await message.Sender.Invoke(Mock.Of<IWhatsAppClient>(), default));
    }

    [Fact]
    public void RoundtripTextParameter()
    {
        var parameter = new TextParameter("Hello", "message");
        var json = JsonSerializer.Serialize(parameter, JsonContext.DefaultOptions);

        var deserialized = JsonSerializer.Deserialize<TextParameter>(json, JsonContext.DefaultOptions);
        Assert.NotNull(deserialized);
        var typed = Assert.IsType<TextParameter>(deserialized);
        Assert.Equal("Hello", typed.Value);
        Assert.Equal("message", typed.Name);
    }

    [Fact]
    public void RoundtripTextParameterWithoutName()
    {
        var parameter = new TextParameter("Hello World");
        var json = JsonSerializer.Serialize(parameter, JsonContext.DefaultOptions);

        var deserialized = JsonSerializer.Deserialize<TextParameter>(json, JsonContext.DefaultOptions);
        Assert.NotNull(deserialized);
        var typed = Assert.IsType<TextParameter>(deserialized);
        Assert.Equal("Hello World", typed.Value);
        Assert.Null(typed.Name);
    }

    [Fact]
    public void RoundtripCurrencyParameter()
    {
        var parameter = new CurrencyParameter("$100.00", "USD", 100000);
        var json = JsonSerializer.Serialize(parameter, JsonContext.DefaultOptions);

        var deserialized = JsonSerializer.Deserialize<CurrencyParameter>(json, JsonContext.DefaultOptions);
        Assert.NotNull(deserialized);
        var typed = Assert.IsType<CurrencyParameter>(deserialized);
        Assert.Equal("$100.00", typed.FallbackValue);
        Assert.Equal("USD", typed.Code);
        Assert.Equal(100000, typed.Amount1000);
    }

    [Fact]
    public void RoundtripDateTimeParameter()
    {
        var parameter = new DateTimeParameter("January 1, 2025");
        var json = JsonSerializer.Serialize(parameter, JsonContext.DefaultOptions);

        var deserialized = JsonSerializer.Deserialize<DateTimeParameter>(json, JsonContext.DefaultOptions);
        Assert.NotNull(deserialized);
        var typed = Assert.IsType<DateTimeParameter>(deserialized);
        Assert.Equal("January 1, 2025", typed.FallbackValue);
    }

    [Fact]
    public void RoundtripImageParameterWithId()
    {
        var parameter = new ImageParameter("image123");
        var json = JsonSerializer.Serialize(parameter, JsonContext.DefaultOptions);

        var deserialized = JsonSerializer.Deserialize<ImageParameter>(json, JsonContext.Default.Options);
        Assert.NotNull(deserialized);
        var typed = Assert.IsType<ImageParameter>(deserialized);
        Assert.Equal("image123", typed.Id);
        Assert.Null(typed.Link);
    }

    [Fact]
    public void RoundtripImageParameterWithLink()
    {
        var uri = new Uri("https://example.com/image.jpg");
        var parameter = new ImageParameter(uri);
        var json = JsonSerializer.Serialize(parameter, JsonContext.Default.Options);

        var deserialized = JsonSerializer.Deserialize<ImageParameter>(json, JsonContext.Default.Options);
        Assert.NotNull(deserialized);
        var typed = Assert.IsType<ImageParameter>(deserialized);
        Assert.Null(typed.Id);
        Assert.Equal(uri, typed.Link);
    }

    [Fact]
    public void RoundtripVideoParameterWithId()
    {
        var parameter = new VideoParameter("video456");
        var json = JsonSerializer.Serialize(parameter, JsonContext.DefaultOptions);

        var deserialized = JsonSerializer.Deserialize<VideoParameter>(json, JsonContext.Default.Options);
        Assert.NotNull(deserialized);
        var typed = Assert.IsType<VideoParameter>(deserialized);
        Assert.Equal("video456", typed.Id);
        Assert.Null(typed.Link);
    }

    [Fact]
    public void RoundtripVideoParameterWithLink()
    {
        var uri = new Uri("https://example.com/video.mp4");
        var parameter = new VideoParameter(uri);
        var json = JsonSerializer.Serialize(parameter, JsonContext.DefaultOptions);

        var deserialized = JsonSerializer.Deserialize<VideoParameter>(json, JsonContext.Default.Options);
        Assert.NotNull(deserialized);
        var typed = Assert.IsType<VideoParameter>(deserialized);
        Assert.Null(typed.Id);
        Assert.Equal(uri, typed.Link);
    }

    [Fact]
    public void RoundtripDocumentParameterWithId()
    {
        var parameter = new DocumentParameter("doc789");
        var json = JsonSerializer.Serialize(parameter, JsonContext.DefaultOptions);

        var deserialized = JsonSerializer.Deserialize<DocumentParameter>(json, JsonContext.Default.Options);
        Assert.NotNull(deserialized);
        var typed = Assert.IsType<DocumentParameter>(deserialized);
        Assert.Equal("doc789", typed.Id);
        Assert.Null(typed.Link);
    }

    [Fact]
    public void RoundtripDocumentParameterWithLink()
    {
        var uri = new Uri("https://example.com/document.pdf");
        var parameter = new DocumentParameter(uri);
        var json = JsonSerializer.Serialize(parameter, JsonContext.Default.Options);

        var deserialized = JsonSerializer.Deserialize<DocumentParameter>(json, JsonContext.Default.Options);
        Assert.NotNull(deserialized);
        var typed = Assert.IsType<DocumentParameter>(deserialized);
        Assert.Null(typed.Id);
        Assert.Equal(uri, typed.Link);
    }

    [Fact]
    public void RoundtripLocationParameter()
    {
        var parameter = new LocationParameter(37.483307, -122.148981, "Facebook HQ", "1 Hacker Way, Menlo Park, CA 94025");
        var json = JsonSerializer.Serialize(parameter, JsonContext.Default.Options);

        var deserialized = JsonSerializer.Deserialize<LocationParameter>(json, JsonContext.Default.Options);
        Assert.NotNull(deserialized);
        var typed = Assert.IsType<LocationParameter>(deserialized);
        Assert.Equal(37.483307, typed.Latitude);
        Assert.Equal(-122.148981, typed.Longitude);
        Assert.Equal("Facebook HQ", typed.Name);
        Assert.Equal("1 Hacker Way, Menlo Park, CA 94025", typed.Address);
    }

    [Fact]
    public void RoundtripTemplateParameterPolymorphism()
    {
        // Test that we can serialize/deserialize as the base TemplateParameter type
        TemplateParameter[] parameters = [
            new TextParameter("Hello", "greeting"),
            new CurrencyParameter("€50.00", "EUR", 50000),
            new DateTimeParameter("December 25, 2024"),
            new ImageParameter("img001"),
            new VideoParameter(new Uri("https://example.com/video.mp4")),
            new DocumentParameter("doc002"),
            new LocationParameter(40.7128, -74.0060, "New York City", "New York, NY, USA")
        ];

        var json = JsonSerializer.Serialize(parameters, JsonContext.DefaultOptions);
        var deserialized = JsonSerializer.Deserialize<TemplateParameter[]>(json, JsonContext.DefaultOptions);

        Assert.NotNull(deserialized);
        Assert.Equal(7, deserialized.Length);

        // Verify each parameter type was correctly deserialized
        Assert.IsType<TextParameter>(deserialized[0]);
        Assert.IsType<CurrencyParameter>(deserialized[1]);
        Assert.IsType<DateTimeParameter>(deserialized[2]);
        Assert.IsType<ImageParameter>(deserialized[3]);
        Assert.IsType<VideoParameter>(deserialized[4]);
        Assert.IsType<DocumentParameter>(deserialized[5]);
        Assert.IsType<LocationParameter>(deserialized[6]);

        // Verify the values are preserved
        var textParam = (TextParameter)deserialized[0];
        Assert.Equal("Hello", textParam.Value);
        Assert.Equal("greeting", textParam.Name);

        var currencyParam = (CurrencyParameter)deserialized[1];
        Assert.Equal("€50.00", currencyParam.FallbackValue);
        Assert.Equal("EUR", currencyParam.Code);
        Assert.Equal(50000, currencyParam.Amount1000);

        var videoParam = (VideoParameter)deserialized[4];
        Assert.Equal("https://example.com/video.mp4", videoParam.Link?.AbsoluteUri);
        Assert.Null(videoParam.Id);
    }

    [Fact]
    public void VerifyGenericConverterArchitecture()
    {
        // Test that concrete type deserialization works with the new generic converter architecture
        var imageJson = @"{""type"":""image"",""image"":{""id"":""test123""}}";
        var videoJson = @"{""type"":""video"",""video"":{""link"":""https://example.com/test.mp4""}}";
        var textJson = @"{""type"":""text"",""text"":""Hello World"",""parameter_name"":""greeting""}";

        // Test direct concrete type deserialization
        var image = JsonSerializer.Deserialize<ImageParameter>(imageJson, JsonContext.DefaultOptions);
        Assert.NotNull(image);
        Assert.Equal("test123", image.Id);
        Assert.Null(image.Link);

        var video = JsonSerializer.Deserialize<VideoParameter>(videoJson, JsonContext.DefaultOptions);
        Assert.NotNull(video);
        Assert.Equal("https://example.com/test.mp4", video.Link?.AbsoluteUri);
        Assert.Null(video.Id);

        var text = JsonSerializer.Deserialize<TextParameter>(textJson, JsonContext.DefaultOptions);
        Assert.NotNull(text);
        Assert.Equal("Hello World", text.Value);
        Assert.Equal("greeting", text.Name);

        // Test polymorphic deserialization still works
        var imageBase = JsonSerializer.Deserialize<TemplateParameter>(imageJson, JsonContext.DefaultOptions);
        Assert.IsType<ImageParameter>(imageBase);

        var videoBase = JsonSerializer.Deserialize<TemplateParameter>(videoJson, JsonContext.Default.Options);
        Assert.IsType<VideoParameter>(videoBase);

        var textBase = JsonSerializer.Deserialize<TemplateParameter>(textJson, JsonContext.Default.Options);
        Assert.IsType<TextParameter>(textBase);
    }

    [Fact]
    public void VerifyUnifiedMediaConverterArchitecture()
    {
        // Test that all three media parameter types use the unified MediaParameterConverter base class
        var imageIdJson = @"{""type"":""image"",""image"":{""id"":""img123""}}";
        var imageLinkJson = @"{""type"":""image"",""image"":{""link"":""https://example.com/image.jpg""}}";

        var videoIdJson = @"{""type"":""video"",""video"":{""id"":""vid456""}}";
        var videoLinkJson = @"{""type"":""video"",""video"":{""link"":""https://example.com/video.mp4""}}";

        var docIdJson = @"{""type"":""document"",""document"":{""id"":""doc789""}}";
        var docLinkJson = @"{""type"":""document"",""document"":{""link"":""https://example.com/document.pdf""}}";

        // Test ID-based creation for all media types
        var imageFromId = JsonSerializer.Deserialize<ImageParameter>(imageIdJson, JsonContext.DefaultOptions);
        Assert.NotNull(imageFromId);
        Assert.Equal("img123", imageFromId.Id);
        Assert.Null(imageFromId.Link);

        var videoFromId = JsonSerializer.Deserialize<VideoParameter>(videoIdJson, JsonContext.DefaultOptions);
        Assert.NotNull(videoFromId);
        Assert.Equal("vid456", videoFromId.Id);
        Assert.Null(videoFromId.Link);

        var docFromId = JsonSerializer.Deserialize<DocumentParameter>(docIdJson, JsonContext.DefaultOptions);
        Assert.NotNull(docFromId);
        Assert.Equal("doc789", docFromId.Id);
        Assert.Null(docFromId.Link);

        // Test Link-based creation for all media types
        var imageFromLink = JsonSerializer.Deserialize<ImageParameter>(imageLinkJson, JsonContext.DefaultOptions);
        Assert.NotNull(imageFromLink);
        Assert.Null(imageFromLink.Id);
        Assert.Equal("https://example.com/image.jpg", imageFromLink.Link?.AbsoluteUri);

        var videoFromLink = JsonSerializer.Deserialize<VideoParameter>(videoLinkJson, JsonContext.Default.Options);
        Assert.NotNull(videoFromLink);
        Assert.Null(videoFromLink.Id);
        Assert.Equal("https://example.com/video.mp4", videoFromLink.Link?.AbsoluteUri);

        var docFromLink = JsonSerializer.Deserialize<DocumentParameter>(docLinkJson, JsonContext.Default.Options);
        Assert.NotNull(docFromLink);
        Assert.Null(docFromLink.Id);
        Assert.Equal("https://example.com/document.pdf", docFromLink.Link?.AbsoluteUri);
    }

    [Fact]
    public void VerifyUnifiedPropertyNameArchitecture()
    {
        // Test that the PropertyName concept works correctly for all parameter types
        var currencyJson = @"{""type"":""currency"",""currency"":{""fallback_value"":""$100.00"",""code"":""USD"",""amount_1000"":100000}}";
        var dateTimeJson = @"{""type"":""date_time"",""date_time"":{""fallback_value"":""January 1, 2025""}}";
        var locationJson = @"{""type"":""location"",""location"":{""latitude"":37.483307,""longitude"":-122.148981,""name"":""Facebook HQ"",""address"":""1 Hacker Way, Menlo Park, CA 94025""}}";
        var imageJson = @"{""type"":""image"",""image"":{""id"":""img123""}}";
        var textJson = @"{""type"":""text"",""text"":""Hello World"",""parameter_name"":""greeting""}";

        // Test that each converter gets the correct JsonElement based on PropertyName
        var currency = JsonSerializer.Deserialize<CurrencyParameter>(currencyJson, JsonContext.DefaultOptions);
        Assert.NotNull(currency);
        Assert.Equal("$100.00", currency.FallbackValue);
        Assert.Equal("USD", currency.Code);
        Assert.Equal(100000, currency.Amount1000);

        var dateTime = JsonSerializer.Deserialize<DateTimeParameter>(dateTimeJson, JsonContext.DefaultOptions);
        Assert.NotNull(dateTime);
        Assert.Equal("January 1, 2025", dateTime.FallbackValue);

        var location = JsonSerializer.Deserialize<LocationParameter>(locationJson, JsonContext.DefaultOptions);
        Assert.NotNull(location);
        Assert.Equal(37.483307, location.Latitude);
        Assert.Equal(-122.148981, location.Longitude);
        Assert.Equal("Facebook HQ", location.Name);
        Assert.Equal("1 Hacker Way, Menlo Park, CA 94025", location.Address);

        var image = JsonSerializer.Deserialize<ImageParameter>(imageJson, JsonContext.Default.Options);
        Assert.NotNull(image);
        Assert.Equal("img123", image.Id);
        Assert.Null(image.Link);

        var text = JsonSerializer.Deserialize<TextParameter>(textJson, JsonContext.Default.Options);
        Assert.NotNull(text);
        Assert.Equal("Hello World", text.Value);
        Assert.Equal("greeting", text.Name);
    }

    [Fact]
    public void MessageTemplateWithHeaderAndBodySerializesCorrectly()
    {
        var template = new MessageTemplate("meeting", "es")
        {
            Header = new HeaderComponent(new LocationParameter(37.483307, -122.148981, "Pablo Morales", "1 Hacker Way, Menlo Park, CA 94025")),
            Body = new BodyComponent([
                new TextParameter("kzu", "who"),
                new TextParameter("office", "where"),
                new TextParameter("15'", "when")
            ])
        };

        var json = JsonSerializer.Serialize(template, new JsonSerializerOptions { WriteIndented = true });

        output.WriteLine(json);

        // Verify that both header and body components are present
        var templateObj = JsonSerializer.Deserialize<JsonElement>(json);
        Assert.True(templateObj.TryGetProperty("components", out var components));

        var componentArray = components.EnumerateArray().ToArray();
        Assert.Equal(2, componentArray.Length);

        // Check that we have both header and body
        var types = componentArray.Select(c => c.GetProperty("type").GetString()).ToArray();
        Assert.Contains("header", types);
        Assert.Contains("body", types);

        Assert.Equal("meeting", templateObj.GetProperty("name").GetString());
        Assert.Equal("es", templateObj.GetProperty("language").GetProperty("code").GetString());
    }

    [Fact]
    public void MessageTemplateProducesSameJsonAsAnonymousObject()
    {
        // Create MessageTemplate instance
        var messageTemplate = new MessageTemplate("reminder", "es")
        {
            Body = new BodyComponent([
                new TextParameter("🦷", "emoji"),
                new TextParameter("Dentista", "text"),
                new TextParameter("3pm", "when")
            ])
        };

        // Create anonymous object like in the test example
        var anonymousTemplate = new
        {
            name = "reminder",
            language = new
            {
                code = "es"
            },
            components = new object[]
            {
                new
                {
                    type = "body",
                    parameters = new object[]
                    {
                        new TextParameter("🦷", "emoji"),
                        new TextParameter("Dentista", "text"),
                        new TextParameter("3pm", "when")
                    }
                }
            }
        };

        var messageTemplateJson = JsonSerializer.Serialize(messageTemplate, new JsonSerializerOptions { WriteIndented = true });
        var anonymousJson = JsonSerializer.Serialize(anonymousTemplate, new JsonSerializerOptions { WriteIndented = true });

        output.WriteLine("MessageTemplate JSON:");
        output.WriteLine(messageTemplateJson);
        output.WriteLine("\nAnonymous Object JSON:");
        output.WriteLine(anonymousJson);

        // Parse both JSONs and compare structure
        var messageTemplateObj = JsonSerializer.Deserialize<JsonElement>(messageTemplateJson);
        var anonymousObj = JsonSerializer.Deserialize<JsonElement>(anonymousJson);

        // Compare key properties
        Assert.Equal(
            messageTemplateObj.GetProperty("name").GetString(),
            anonymousObj.GetProperty("name").GetString());

        Assert.Equal(
            messageTemplateObj.GetProperty("language").GetProperty("code").GetString(),
            anonymousObj.GetProperty("language").GetProperty("code").GetString());

        // Both should have components array with same structure
        var messageComponents = messageTemplateObj.GetProperty("components").EnumerateArray().ToArray();
        var anonymousComponents = anonymousObj.GetProperty("components").EnumerateArray().ToArray();
        Assert.Equal(messageComponents.Length, anonymousComponents.Length);
    }


    [Fact]
    public void MessageTemplateWorksWithJsonContext()
    {
        var template = new MessageTemplate("test", "en")
        {
            Header = new HeaderComponent(new ImageParameter("image123")),
            Body = new BodyComponent([
                new TextParameter("Hello", "greeting"),
                new CurrencyParameter("$10", "USD", 10000)
            ])
        };

        // Test with the library's JsonContext
        var jsonWithContext = JsonSerializer.Serialize(template, JsonContext.DefaultOptions);
        var deserializedWithContext = JsonSerializer.Deserialize<MessageTemplate>(jsonWithContext, JsonContext.DefaultOptions);

        Assert.NotNull(deserializedWithContext);
        Assert.Equal(template.Name, deserializedWithContext.Name);
        Assert.Equal(template.Language, deserializedWithContext.Language);
        Assert.NotNull(deserializedWithContext.Header);
        Assert.NotNull(deserializedWithContext.Body);
        Assert.Equal(2, deserializedWithContext.Body.Parameters.Count);

        output.WriteLine("JSON with JsonContext:");
        output.WriteLine(jsonWithContext);
    }

    [Fact]
    public void MessageTemplateSerializesToExpectedFormat()
    {
        var template = new MessageTemplate("reminder", "es")
        {
            Header = new HeaderComponent(new TextParameter("Recordatorio", "title")),
            Body = new BodyComponent([
                new TextParameter("🦷", "emoji"),
                new TextParameter("Dentista", "text"),
                new TextParameter("3pm", "when")
            ])
        };

        var json = JsonSerializer.Serialize(template, JsonContext.DefaultOptions);

        output.WriteLine(json);

        // Verify the JSON structure matches the expected format
        var expected = """
            {
              "name": "reminder",
              "language": {
                "code": "es"
              },
              "components": [
                { 
                  "type": "header",
                  "parameters": [
                    {
                      "type": "text",
                      "text": "Recordatorio",
                      "parameter_name": "title"
                    }
                  ]
                },
                {
                  "type": "body",
                  "parameters": [
                    {
                      "type": "text",
                      "text": "🦷",
                      "parameter_name": "emoji"
                    },
                    {
                      "type": "text",
                      "text": "Dentista",
                      "parameter_name": "text"
                    },
                    {
                      "type": "text",
                      "text": "3pm",
                      "parameter_name": "when"
                    }
                  ]
                }
              ]
            }
            """;

        var expectedObj = JsonSerializer.Deserialize<object>(expected);
        var actualObj = JsonSerializer.Deserialize<object>(json);

        Assert.Equal(JsonSerializer.Serialize(expectedObj), JsonSerializer.Serialize(actualObj));
    }

    [Fact]
    public void MessageTemplateDeserializesFromExpectedFormat()
    {
        var json = """
            {
              "name": "reminder",
              "language": {
                "code": "es"
              },
              "components": [
                {
                  "type": "body",
                  "parameters": [
                    {
                      "type": "text",
                      "text": "🦷",
                      "parameter_name": "emoji"
                    },
                    {
                      "type": "text",
                      "text": "Dentista",
                      "parameter_name": "text"
                    }
                  ]
                }
              ]
            }
            """;

        var template = JsonSerializer.Deserialize<MessageTemplate>(json);

        Assert.NotNull(template);
        Assert.Equal("reminder", template.Name);
        Assert.Equal("es", template.Language);
        Assert.NotNull(template.Body);
        Assert.Null(template.Header);
        Assert.Equal(2, template.Body.Parameters.Count);

        var firstParam = template.Body.Parameters[0] as TextParameter;
        Assert.NotNull(firstParam);
        Assert.Equal("🦷", firstParam.Value);
        Assert.Equal("emoji", firstParam.Name);
    }

    [Fact]
    public void MessageTemplateWithTextAndPayloadButtonsSerialization()
    {
        var template = new MessageTemplate("test", "en")
        {
            Buttons =
            [
                ButtonComponent.Payload("foo"),
                ButtonComponent.Url("bar"),
                ButtonComponent.Catalog()
            ]
        };

        var json = JsonSerializer.Serialize(template, JsonContext.DefaultOptions);

        output.WriteLine(json);

        // Verify the JSON structure matches the expected format

    }

    [Fact]
    public void RoundtripCallToFlowResponse()
    {
        var expected = new CallToFlowResponse("1234", "5687", "text", "action", new FlowParameters("flow"));

        var json = JsonSerializer.Serialize<IMessage>(expected, JsonContext.DefaultOptions);
        output.WriteLine(json);

        var actual = JsonSerializer.Deserialize<IMessage>(json, JsonContext.DefaultOptions);

        Assert.NotNull(actual);
        var value = Assert.IsType<CallToFlowResponse>(actual);

        Assert.Equal(expected, value);
    }

    [Fact]
    public void RoundtripCallToActionResponse()
    {
        var expected = new CallToActionResponse("1234", "5687", "text", "action", "http://foo");

        var json = JsonSerializer.Serialize<IMessage>(expected, JsonContext.DefaultOptions);
        output.WriteLine(json);

        var actual = JsonSerializer.Deserialize<IMessage>(json, JsonContext.DefaultOptions);

        Assert.NotNull(actual);
        var value = Assert.IsType<CallToActionResponse>(actual);

        Assert.Equal(expected, value);
    }

    [Fact]
    public void MatchMessageJsonTemplate()
    {
        var json =
            """
            {
              "messaging_product": "whatsapp",
              "recipient_type": "individual",
              "to": "PHONE_NUMBER",
              "type": "template",
              "template": {
                "name": "TEMPLATE_NAME",
                "language": {
                  "code": "LANGUAGE_AND_LOCALE_CODE"
                },
                "components": [
                  {
                    "type": "header",
                    "parameters": [
                      {
                        "type": "image",
                        "image": {
                          "link": "https://foo.com/"
                        }
                      }
                    ]
                  },
                  {
                    "type": "body",
                    "parameters": [
                      {
                        "type": "text",
                        "text": "TEXT_STRING"
                      },
                      {
                        "type": "currency",
                        "currency": {
                          "fallback_value": "VALUE",
                          "code": "USD",
                          "amount_1000": 123
                        }
                      },
                      {
                        "type": "date_time",
                        "date_time": {
                          "fallback_value": "MONTH DAY, YEAR"
                        }
                      }
                    ]
                  },
                  {
                    "type": "button",
                    "sub_type": "quick_reply",
                    "index": "0",
                    "parameters": [
                      {
                        "type": "payload",
                        "payload": "PAYLOAD"
                      }
                    ]
                  },
                  {
                    "type": "button",
                    "sub_type": "quick_reply",
                    "index": "1",
                    "parameters": [
                      {
                        "type": "payload",
                        "payload": "PAYLOAD"
                      }
                    ]
                  }
                ]
              }
            }
            """;

        // Extract just the template portion for MessageTemplate deserialization
        var templateJson = JsonSerializer.Deserialize<JsonElement>(json).GetProperty("template").GetRawText();
        var template = JsonSerializer.Deserialize<MessageTemplate>(templateJson, JsonContext.DefaultOptions);

        // Verify basic template properties
        Assert.NotNull(template);
        Assert.Equal("TEMPLATE_NAME", template.Name);
        Assert.Equal("LANGUAGE_AND_LOCALE_CODE", template.Language);

        // Verify header component
        Assert.NotNull(template.Header);
        Assert.Single(template.Header.Parameters);
        var headerParam = Assert.IsType<ImageParameter>(template.Header.Parameters[0]);
        Assert.Equal("https://foo.com/", headerParam.Link?.OriginalString);
        Assert.Null(headerParam.Id);

        // Verify body component
        Assert.NotNull(template.Body);
        Assert.Equal(3, template.Body.Parameters.Count);

        // Verify body parameters
        var textParam = Assert.IsType<TextParameter>(template.Body.Parameters[0]);
        Assert.Equal("TEXT_STRING", textParam.Value);
        Assert.Null(textParam.Name); // No parameter_name provided in JSON

        var currencyParam = Assert.IsType<CurrencyParameter>(template.Body.Parameters[1]);
        Assert.Equal("VALUE", currencyParam.FallbackValue);
        Assert.Equal("USD", currencyParam.Code);
        Assert.Equal(123, currencyParam.Amount1000);

        var dateTimeParam = Assert.IsType<DateTimeParameter>(template.Body.Parameters[2]);
        Assert.Equal("MONTH DAY, YEAR", dateTimeParam.FallbackValue);

        // Verify buttons
        Assert.NotNull(template.Buttons);
        Assert.Equal(2, template.Buttons.Count);

        // Verify first button
        var firstButton = template.Buttons[0];
        Assert.Equal(ButtonSubType.QuickReply, firstButton.SubType);
        Assert.NotNull(firstButton.Parameters);
        Assert.Single(firstButton.Parameters);
        var firstPayload = Assert.IsType<PayloadButtonParameter>(firstButton.Parameters[0]);
        Assert.Equal("PAYLOAD", firstPayload.Payload);

        // Verify second button  
        var secondButton = template.Buttons[1];
        Assert.Equal(ButtonSubType.QuickReply, secondButton.SubType);
        Assert.NotNull(secondButton.Parameters);
        Assert.Single(secondButton.Parameters);
        var secondPayload = Assert.IsType<PayloadButtonParameter>(secondButton.Parameters[0]);
        Assert.Equal("PAYLOAD", secondPayload.Payload);
    }
}
