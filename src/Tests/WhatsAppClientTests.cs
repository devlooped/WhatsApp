using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit.Abstractions;

namespace Devlooped.WhatsApp;

public class WhatsAppClientTests(ITestOutputHelper output)
{
    [Fact]
    public async Task ThrowsIfNoConfiguredNumberAsync()
    {
        var client = WhatsAppClient.Create(MockHttpClientFactory.Default, new MetaOptions
        {
            VerifyToken = "asdf"
        }, MockLogger.Create<WhatsAppClient>());

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => client.SendAync("1234", new { }));

        Assert.Equal("from", ex.ParamName);
    }

    [SecretsFact("Meta:VerifyToken", "SendFrom", "SendTo")]
    public async Task SendsMessageAsync()
    {
        var (configuration, client) = Initialize();

        var result = await client.SendTextAync(configuration["SendFrom"]!, configuration["SendTo"]!, "Hi there!");

        Assert.True(result);
    }

    [SecretsFact("Meta:VerifyToken", "SendFrom", "SendTo")]
    public async Task SendsButtonAsync()
    {
        var (configuration, client) = Initialize();

        // Send an interactive message with three buttons showcasing the payload/value 
        // being different than the button text
        var result = await client.SendAync(configuration["SendFrom"]!, new
        {
            messaging_product = "whatsapp",
            recipient_type = "individual",
            to = configuration["SendTo"]!,
            type = "interactive",
            interactive = new
            {
                type = "button",
                body = new
                {
                    text = "Choose a button"
                },
                action = new
                {
                    buttons = new[]
                    {
                        new { type = "reply", reply = new { id = "1", title = "Button 1" } },
                        new { type = "reply", reply = new { id = "2", title = "Button 2" } },
                        new { type = "reply", reply = new { id = "3", title = "Button 3" } }
                    }
                }
            }
        });

        Assert.True(result);
    }

    (IConfiguration configuration, WhatsAppClient client) Initialize()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<WhatsAppClientTests>()
            .Build();

        var collection = new ServiceCollection()
            .AddSingleton<ILoggerFactory>(new MockLogger(output))
            .AddHttpClient()
            .AddSingleton<IConfiguration>(configuration);

        collection.AddOptions<MetaOptions>()
            .BindConfiguration("Meta")
            .ValidateDataAnnotations();

        collection.AddSingleton<WhatsAppClient>();

        var services = collection.BuildServiceProvider();
        return (configuration, services.GetRequiredService<WhatsAppClient>());
    }
}
