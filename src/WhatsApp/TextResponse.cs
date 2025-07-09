namespace Devlooped.WhatsApp;

/// <summary>
/// Represents a response containing text and optional interactive buttons,  which can be sent as a reply to a message. 
/// </summary>
/// <remarks>This response type allows sending a text message with up to three optional buttons  for user
/// interaction. If no buttons are provided, the response will consist of  only the text message.</remarks>
public record TextResponse : Response
{
    // If this variable is not null, it means the originating message came from WhatsApp
    // and there's an ongoing conversation with the CLI simultaneously.
    readonly CompositeService? service;
    Func<IWhatsAppClient, string, string, CancellationToken, Task<string?>> sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="TextResponse"/> class with the specified parameters.
    /// </summary>
    /// <param name="serviceId">The identifier of the service handling the message.</param>
    /// <param name="userNumber">The phone number of the recipient in international format.</param>
    /// <param name="context">Optional identifier of the message to which this response may be a reply to.</param>
    /// <param name="text">The text content of the response message.</param>
    /// <param name="button1">An optional button to include in the response for user interaction.</param>
    /// <param name="button2">An optional second button to include in the response for user interaction.</param>
    /// <param name="button3">An optional third button to include in the response for user interaction.</param>
    public TextResponse(string serviceId, string userNumber, string? context, string? text, Button? button1 = default, Button? button2 = default, Button? button3 = default)
        : base(serviceId, userNumber, context)
    {
        sender = context == null ? SendTextAsync : SendReplyAsync;

        Text = text;
        Button1 = button1;
        Button2 = button2;
        Button3 = button3;
    }

    /// <summary>
    /// Gets or sets the text content of the response message.
    /// </summary>
    public string? Text { get; init; }
    /// <summary>
    /// An optional button to include in the response for user interaction.
    /// </summary>
    public Button? Button1 { get; init; }
    /// <summary>
    /// An optional second button to include in the response for user interaction.
    /// </summary>
    public Button? Button2 { get; init; }
    /// <summary>
    /// An optional third button to include in the response for user interaction.
    /// </summary>
    public Button? Button3 { get; init; }

    internal TextResponse(Service service, string userNumber, string? context, string? text, Button? button1 = default, Button? button2 = default, Button? button3 = default)
        : this(service.Id, userNumber, context, text, button1, button2, button3)
        => this.service = service as CompositeService;

    /// <inheritdoc/>
    protected override async Task<string?> SendCoreAsync(IWhatsAppClient client, CancellationToken cancellation = default)
    {
        if (service != null)
            await sender(client, service.Secondary.Id, this.ConsoleText ?? Text ?? "", cancellation);

        // If service is null, it's either a WhatsApp regular without CLI, or it's pure CLI.
        // In the former case, we don't want to send messages that are CLI-only if the service id 
        // is not actually a CLI service.
        if (this.ConsoleOnly == true && !ServiceId.IsCLI())
            return null;

        // It may not be CLI-only but still provide a CLI-enhanced text.
        return await sender(client, ServiceId,
            // Automatically pick the CLI version of the text if sending to the CLI
            ServiceId.IsCLI() ? this.ConsoleText ?? Text ?? "" : Text ?? "", cancellation);
    }

    Task<string?> SendReplyAsync(IWhatsAppClient client, string serviceId, string text, CancellationToken cancellation)
    {
        if (Button1 != null)
        {
            if (Button2 == null)
                return client.ReplyAsync(serviceId, UserNumber, Context!, text, Button1, cancellation);
            else if (Button3 == null)
                return client.ReplyAsync(serviceId, UserNumber, Context!, text, Button1, Button2, cancellation);
            else
                return client.ReplyAsync(serviceId, UserNumber, Context!, text, Button1, Button2, Button3, cancellation);
        }
        else
        {
            return client.ReplyAsync(serviceId, UserNumber, Context!, text, cancellation);
        }
    }

    Task<string?> SendTextAsync(IWhatsAppClient client, string serviceId, string text, CancellationToken cancellation)
    {
        if (Button1 != null)
        {
            if (Button2 == null)
                return client.SendAsync(serviceId, UserNumber, text, Button1, cancellation);
            else if (Button3 == null)
                return client.SendAsync(serviceId, UserNumber, text, Button1, Button2, cancellation);
            else
                return client.SendAsync(serviceId, UserNumber, text, Button1, Button2, Button3, cancellation);
        }
        else
        {
            return client.SendAsync(serviceId, UserNumber, text, cancellation);
        }
    }
}
