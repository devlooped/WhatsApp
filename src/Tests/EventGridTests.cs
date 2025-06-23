using Azure;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Configuration;

namespace Devlooped.WhatsApp;

public class EventGridTests
{
    [SecretsFact("EventGrid:Topic", "EventGrid:Key", "SendFrom", "SendTo")]
    public async Task SendEvent()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<EventGridTests>()
            .Build();

        var client = new EventGridPublisherClient(
            new Uri(configuration["EventGrid:Topic"]!),
            new AzureKeyCredential(configuration["EventGrid:Key"]!));

        var processor = new EventGridProcessor(client, new EventGridOptions());

        await processor.EnqueueAsync(
            $$"""
            {
                "Content": {
                    "$type": "text",
                    "Text": "Is it running?"
                },
                "Id": "wamid.HBgNNTQ5MTE1OTI3ODI4MhUCABIYIEYyQ0U5N0E0MDA5MkU4MUU5RkU1RERCMzE5Q0QzNjk3AA==",
                "Service": {
                    "Id": "{{configuration["SendFrom"]}}",
                    "Number": "{{configuration["SendFromNumber"]}}"
                },
                "User": {
                    "Name": "Test",
                    "Number": "{{configuration["SendTo"]}}"
                },
                "Timestamp": 1749722446,
                "notification": "539235785933710",
                "Number": "{{configuration["SendTo"]}}"
            }
            """);
    }
}
