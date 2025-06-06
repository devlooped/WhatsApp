namespace Devlooped.WhatsApp;

/// <summary>
/// Usability extensions for common messaging scenarios for WhatsApp.
/// </summary>
public static partial class WhatsAppClientExtensions
{
    /// <summary>
    /// Creates an authenticated HTTP client for the given service number.
    /// </summary>
    public static HttpClient CreateHttp(this IWhatsAppClient client, Service service)
        => client.CreateHttp(service.Id);

    /// <summary>
    /// Creates an authenticated HTTP client for the service number that received the given message.
    /// </summary>
    public static HttpClient CreateHttp(this IWhatsAppClient client, Message message)
        => client.CreateHttp(message.To.Id);

    /// <summary>
    /// Marks the message as read. Happens automatically when the <see cref="AzureFunctions.Message(Microsoft.AspNetCore.Http.HttpRequest)"/> 
    /// webhook endpoint is invoked with a message.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="message">The message to mark as read.</param>
    /// <param name="cancellation">The cancellation token.</param>
    public static Task MarkReadAsync(this IWhatsAppClient client, UserMessage message, CancellationToken cancellation = default)
        => MarkReadAsync(client, message.To.Id, message.Id, cancellation);

    /// <summary>
    /// Marks the message as read. Happens automatically when the <see cref="AzureFunctions.Message(Microsoft.AspNetCore.Http.HttpRequest)"/> 
    /// webhook endpoint is invoked with a message.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="from">The service number to send the mark read through.</param>
    /// <param name="messageId">The message identifier to mark as read.</param>
    /// <param name="cancellation">The cancellation token.</param>
    public static Task MarkReadAsync(this IWhatsAppClient client, string from, string messageId, CancellationToken cancellation = default)
        => client.SendAsync(from, new
        {
            messaging_product = "whatsapp",
            status = "read",
            message_id = messageId,
        }, cancellation);

    /// <summary>
    /// Reacts to a message.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="message">The message to react to.</param>
    /// <param name="emoji">The reaction emoji.</param>
    /// <param name="cancellation">The cancellation token.</param>
    /// <see cref="https://developers.facebook.com/docs/whatsapp/cloud-api/reference/messages/#reaction-object"/>
    public static Task ReactAsync(this IWhatsAppClient client, UserMessage message, string emoji, CancellationToken cancellation = default)
        => ReactAsync(client, message.To.Id, message.From.Number, message.Id, emoji, cancellation);

    /// <summary>
    /// Reacts to a message.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="from">The service number to send the reaction through.</param>
    /// <param name="to">The user phone number to send the reaction to.</param>
    /// <param name="messageId">The message identifier to react to.</param>
    /// <param name="emoji">The reaction emoji.</param>
    /// <param name="cancellation">The cancellation token.</param>
    /// <see cref="https://developers.facebook.com/docs/whatsapp/cloud-api/reference/messages/#reaction-object"/>
    public static Task ReactAsync(this IWhatsAppClient client, string from, string to, string messageId, string emoji, CancellationToken cancellation = default)
        => client.SendAsync(from, new
        {
            messaging_product = "whatsapp",
            recipient_type = "individual",
            to = NormalizeNumber(to),
            type = "reaction",
            reaction = new
            {
                message_id = messageId,
                emoji
            }
        }, cancellation);

    /// <summary>
    /// Sends a template message.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="from">The service number to send the template through.</param>
    /// <param name="to">The user phone number to send the template message to.</param>
    /// <param name="template">The raw template object to serialize and send, including name, language and other properties.</param>
    /// <param name="cancellation">Cancellation token for the async operation.</param>
    /// <see cref="https://developers.facebook.com/docs/whatsapp/api/messages/message-templates#supported-languages"/>
    /// <see cref="https://developers.facebook.com/docs/whatsapp/cloud-api/reference/messages/#template-object"/>
    /// <see cref="https://developers.facebook.com/docs/whatsapp/cloud-api/reference/messages/#components-object"/>
    public static Task SendTemplateAsync(this IWhatsAppClient client, string from, string to, object template, CancellationToken cancellation = default)
        => client.SendAsync(from, new
        {
            messaging_product = "whatsapp",
            to = NormalizeNumber(to),
            type = "template",
            template
        }, cancellation);

    /// <summary>
    /// Replies to a user message.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="message">The message to reply to.</param>
    /// <param name="reply">The text message to respond with.</param>
    /// <param name="cancellation">The cancellation token.</param>
    /// <returns>The identifier of the reply.</returns>
    /// <see cref="https://developers.facebook.com/docs/whatsapp/cloud-api/reference/messages/#text-object"/>
    public static Task<string?> ReplyAsync(this IWhatsAppClient client, string number, string service, string replyTo, string reply, CancellationToken cancellation = default)
        => client.SendAsync(service, new
        {
            messaging_product = "whatsapp",
            preview_url = false,
            recipient_type = "individual",
            to = NormalizeNumber(number),
            type = "text",
            context = new
            {
                message_id = replyTo
            },
            text = new
            {
                body = reply
            }
        }, cancellation);

    /// <summary>
    /// Replies to a user message with an additional interactive button.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="message">The message to reply to.</param>
    /// <param name="reply">The text message to respond with.</param>
    /// <param name="button">Interactive button for users to reply.</param>
    /// <param name="cancellation">The cancellation token.</param>
    /// <returns>The identifier of the reply message.</returns>
    /// <see cref="https://developers.facebook.com/docs/whatsapp/cloud-api/reference/messages/#interactive-object"/>
    public static Task<string?> ReplyAsync(this IWhatsAppClient client, string number, string service, string replyTo, string text, Button button, CancellationToken cancellation = default)
        => client.SendAsync(service, new
        {
            messaging_product = "whatsapp",
            preview_url = false,
            recipient_type = "individual",
            to = NormalizeNumber(number),
            type = "interactive",
            context = new
            {
                message_id = replyTo
            },
            interactive = new
            {
                type = "button",
                body = new
                {
                    text = text
                },
                action = new
                {
                    buttons = new[]
                    {
                        new { type = "reply", reply = new { id = button.Id, title = button.Title } },
                    }
                }
            }
        }, cancellation);

    /// <summary>
    /// Replies to a user message with a additional interactive buttons.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="message">The message to reply to.</param>
    /// <param name="reply">The text message to respond with.</param>
    /// <param name="button1">Interactive button for a user choice.</param>
    /// <param name="button2">Interactive button for a user choice.</param>
    /// <param name="cancellation">The cancellation token.</param>
    /// <returns>The identifier of the reply message.</returns>
    /// <see cref="https://developers.facebook.com/docs/whatsapp/cloud-api/reference/messages/#interactive-object"/>
    public static Task<string?> ReplyAsync(this IWhatsAppClient client, string number, string service, string replyTo, string reply, Button button1, Button button2, CancellationToken cancellation = default)
        => client.SendAsync(service, new
        {
            messaging_product = "whatsapp",
            preview_url = false,
            recipient_type = "individual",
            to = NormalizeNumber(number),
            type = "interactive",
            context = new
            {
                message_id = replyTo
            },
            interactive = new
            {
                type = "button",
                body = new
                {
                    text = reply
                },
                action = new
                {
                    buttons = new[]
                    {
                        new { type = "reply", reply = new { id = button1.Id, title = button1.Title } },
                        new { type = "reply", reply = new { id = button2.Id, title = button2.Title } },
                    }
                }
            }
        }, cancellation);

    /// <summary>
    /// Replies to a user message with a additional interactive buttons.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="message">The message to reply to.</param>
    /// <param name="reply">The text message to respond with.</param>
    /// <param name="button1">Interactive button for a user choice.</param>
    /// <param name="button2">Interactive button for a user choice.</param>
    /// <param name="button3">Interactive button for a user choice.</param>
    /// <param name="cancellation">The cancellation token.</param>
    /// <returns>The identifier of the reply message.</returns>
    /// <see cref="https://developers.facebook.com/docs/whatsapp/cloud-api/reference/messages/#interactive-object"/>
    public static Task<string?> ReplyAsync(this IWhatsAppClient client, UserMessage message, string reply, Button button1, Button button2, Button button3, CancellationToken cancellation = default)
        => client.SendAsync(message.To.Id, new
        {
            messaging_product = "whatsapp",
            preview_url = false,
            recipient_type = "individual",
            to = NormalizeNumber(message.From.Number),
            type = "interactive",
            context = new
            {
                message_id = message.Id
            },
            interactive = new
            {
                type = "button",
                body = new
                {
                    text = reply
                },
                action = new
                {
                    buttons = new[]
                    {
                        new { type = "reply", reply = new { id = button1.Id, title = button1.Title } },
                        new { type = "reply", reply = new { id = button2.Id, title = button2.Title } },
                        new { type = "reply", reply = new { id = button3.Id, title = button3.Title } },
                    }
                }
            }
        }, cancellation);

    /// <summary>
    /// Replies to the message a user reacted to.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="reaction">The reaction from the user.</param>
    /// <param name="reply">The text message to respond with.</param>
    /// <param name="cancellation">The cancellation token.</param>
    /// <returns>The identifier of the reply message.</returns>
    /// <see cref="https://developers.facebook.com/docs/whatsapp/cloud-api/reference/messages/#text-object"/>
    public static Task<string?> ReplyAsync(this IWhatsAppClient client, ReactionMessage message, string reply, CancellationToken cancellation = default)
        => client.SendAsync(message.To.Id, new
        {
            messaging_product = "whatsapp",
            preview_url = false,
            recipient_type = "individual",
            to = NormalizeNumber(message.From.Number),
            type = "text",
            context = new
            {
                message_id = message.Context
            },
            text = new
            {
                body = reply
            }
        }, cancellation);

    /// <summary>
    /// Sends a text message a user given his incoming message, without making it a reply.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="to">The originating user to send a message to.</param>
    /// <param name="message">The text message to send.</param>
    /// <param name="cancellation">The cancellation token.</param>
    /// <returns>The identifier of the sent message.</returns>
    /// <see cref="https://developers.facebook.com/docs/whatsapp/cloud-api/reference/messages/#text-object"/>
    public static Task<string?> SendAsync(this IWhatsAppClient client, Message to, string message, CancellationToken cancellation = default)
        => SendAsync(client, to.To.Id, to.From.Number, message, cancellation);

    /// <summary>
    /// Sends a text message a user given his incoming message, without making it a reply.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="to">The originating user to send a message to.</param>
    /// <param name="message">The text message to send.</param>
    /// <param name="button">Interactive button for users to reply.</param>
    /// <param name="cancellation">The cancellation token.</param>
    /// <returns>The identifier of the sent message.</returns>
    /// <see cref="https://developers.facebook.com/docs/whatsapp/cloud-api/reference/messages/#interactive-object"/>
    public static Task<string?> SendAsync(this IWhatsAppClient client, Message to, string message, Button button, CancellationToken cancellation = default)
        => SendAsync(client, to.To.Id, to.From.Number, message, button, cancellation);

    /// <summary>
    /// Sends a text message a user given his incoming message, without making it a reply.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="to">The originating user to send a message to.</param>
    /// <param name="message">The text message to send.</param>
    /// <param name="button1">Interactive button for a user choice.</param>
    /// <param name="button2">Interactive button for a user choice.</param>
    /// <param name="cancellation">The cancellation token.</param>
    /// <returns>The identifier of the sent message.</returns>
    /// <see cref="https://developers.facebook.com/docs/whatsapp/cloud-api/reference/messages/#interactive-object"/>
    public static Task<string?> SendAsync(this IWhatsAppClient client, Message to, string message, Button button1, Button button2, CancellationToken cancellation = default)
        => SendAsync(client, to.To.Id, to.From.Number, message, button1, button2, cancellation);

    /// <summary>
    /// Sends a text message a user given his incoming message, without making it a reply.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="to">The originating user to send a message to.</param>
    /// <param name="message">The text message to send.</param>
    /// <param name="button1">Interactive button for a user choice.</param>
    /// <param name="button2">Interactive button for a user choice.</param>
    /// <param name="button3">Interactive button for a user choice.</param>
    /// <param name="cancellation">The cancellation token.</param>
    /// <returns>The identifier of the sent message.</returns>
    /// <see cref="https://developers.facebook.com/docs/whatsapp/cloud-api/reference/messages/#interactive-object"/>
    public static Task<string?> SendAsync(this IWhatsAppClient client, Message to, string message, Button button1, Button button2, Button button3, CancellationToken cancellation = default)
        => SendAsync(client, to.To.Id, to.From.Number, message, button1, button2, button3, cancellation);

    /// <summary>
    /// Sends a text message a user.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="from">The service number to send the message through.</param>
    /// <param name="to">The user phone number to send the message to.</param>
    /// <param name="message">The text message to send.</param>
    /// <param name="cancellation">The cancellation token.</param>
    /// <returns>The identifier of the sent message.</returns>
    public static Task<string?> SendAsync(this IWhatsAppClient client, string from, string to, string message, CancellationToken cancellation = default)
        => client.SendAsync(from, new
        {
            messaging_product = "whatsapp",
            preview_url = false,
            recipient_type = "individual",
            to = NormalizeNumber(to),
            type = "text",
            text = new
            {
                body = message
            }
        }, cancellation);

    /// <summary>
    /// Sends a text message a user.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="from">The service number to send the message through.</param>
    /// <param name="to">The user phone number to send the message to.</param>
    /// <param name="message">The text message to send.</param>
    /// <param name="button">Interactive button for users to reply.</param>
    /// <param name="cancellation">The cancellation token.</param>
    /// <returns>The identifier of the sent message.</returns>
    /// <see cref="https://developers.facebook.com/docs/whatsapp/cloud-api/reference/messages/#interactive-object"/>
    public static Task<string?> SendAsync(this IWhatsAppClient client, string from, string to, string message, Button button, CancellationToken cancellation = default)
        => client.SendAsync(from, new
        {
            messaging_product = "whatsapp",
            preview_url = false,
            recipient_type = "individual",
            to = NormalizeNumber(to),
            type = "interactive",
            interactive = new
            {
                type = "button",
                body = new
                {
                    text = message
                },
                action = new
                {
                    buttons = new[]
                    {
                        new { type = "reply", reply = new { id = button.Id, title = button.Title } },
                    }
                }
            }
        }, cancellation);

    /// <summary>
    /// Sends a text message a user.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="from">The service number to send the message through.</param>
    /// <param name="to">The user phone number to send the message to.</param>
    /// <param name="message">The text message to send.</param>
    /// <param name="button1">Interactive button for a user choice.</param>
    /// <param name="button2">Interactive button for a user choice.</param>
    /// <param name="cancellation">The cancellation token.</param>
    /// <returns>The identifier of the sent message.</returns>
    /// <see cref="https://developers.facebook.com/docs/whatsapp/cloud-api/reference/messages/#interactive-object"/>
    public static Task<string?> SendAsync(this IWhatsAppClient client, string from, string to, string message, Button button1, Button button2, CancellationToken cancellation = default)
        => client.SendAsync(from, new
        {
            messaging_product = "whatsapp",
            preview_url = false,
            recipient_type = "individual",
            to = NormalizeNumber(to),
            type = "interactive",
            interactive = new
            {
                type = "button",
                body = new
                {
                    text = message
                },
                action = new
                {
                    buttons = new[]
                    {
                        new { type = "reply", reply = new { id = button1.Id, title = button1.Title } },
                        new { type = "reply", reply = new { id = button2.Id, title = button2.Title } },
                    }
                }
            }
        }, cancellation);

    /// <summary>
    /// Sends a text message a user.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="from">The service number to send the message through.</param>
    /// <param name="to">The user phone number to send the message to.</param>
    /// <param name="message">The text message to send.</param>
    /// <param name="button1">Interactive button for a user choice.</param>
    /// <param name="button2">Interactive button for a user choice.</param>
    /// <param name="button3">Interactive button for a user choice.</param>
    /// <param name="cancellation">The cancellation token.</param>
    /// <returns>The identifier of the sent message.</returns>
    /// <see cref="https://developers.facebook.com/docs/whatsapp/cloud-api/reference/messages/#interactive-object"/>
    public static Task<string?> SendAsync(this IWhatsAppClient client, string from, string to, string message, Button button1, Button button2, Button button3, CancellationToken cancellation = default)
        => client.SendAsync(from, new
        {
            messaging_product = "whatsapp",
            preview_url = false,
            recipient_type = "individual",
            to = NormalizeNumber(to),
            type = "interactive",
            interactive = new
            {
                type = "button",
                body = new
                {
                    text = message
                },
                action = new
                {
                    buttons = new[]
                    {
                        new { type = "reply", reply = new { id = button1.Id, title = button1.Title } },
                        new { type = "reply", reply = new { id = button2.Id, title = button2.Title } },
                        new { type = "reply", reply = new { id = button3.Id, title = button3.Title } },
                    }
                }
            }
        }, cancellation);

    static string NormalizeNumber(string number) =>
        // On the web, we don't get the 9 after 54 \o/
        // so for Argentina numbers, we need to remove the 9.
        number.StartsWith("549", StringComparison.Ordinal) ? "54" + number[3..] : number;
}
