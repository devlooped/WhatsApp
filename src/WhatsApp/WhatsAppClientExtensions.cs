namespace Devlooped.WhatsApp;

/// <summary>
/// Usability extensions for common messaging scenarios for WhatsApp.
/// </summary>
public static class WhatsAppClientExtensions
{
    /// <summary>
    /// Marks the message as read. Happens automatically when the <see cref="AzureFunctions.Message(Microsoft.AspNetCore.Http.HttpRequest)"/> 
    /// webhook endpoint is invoked with a message.
    /// </summary>
    public static Task MarkReadAsync(this IWhatsAppClient client, UserMessage message)
        => MarkReadAsync(client, message.To.Id, message.Id);

    /// <summary>
    /// Marks the message as read. Happens automatically when the <see cref="AzureFunctions.Message(Microsoft.AspNetCore.Http.HttpRequest)"/> 
    /// webhook endpoint is invoked with a message.
    /// </summary>
    public static Task MarkReadAsync(this IWhatsAppClient client, string from, string messageId)
        => client.SendAsync(from, new
        {
            messaging_product = "whatsapp",
            status = "read",
            message_id = messageId,
        });

    /// <summary>
    /// Reacts to a message.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="message">The message to react to.</param>
    /// <param name="emoji">The reaction emoji.</param>
    public static Task ReactAsync(this IWhatsAppClient client, UserMessage message, string emoji)
        => ReactAsync(client, message.To.Id, message.From.Number, message.Id, emoji);

    /// <summary>
    /// Reacts to a message.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="from">The service number to send the reaction through.</param>
    /// <param name="to">The user phone number to send the reaction to.</param>
    /// <param name="messageId">The message identifier to react to.</param>
    /// <param name="emoji">The reaction emoji.</param>
    public static Task ReactAsync(this IWhatsAppClient client, string from, string to, string messageId, string emoji)
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
        });

    /// <summary>
    /// Replies to a user message.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="message">The message to reply to.</param>
    /// <param name="reply">The text message to respond with.</param>
    public static Task ReplyAsync(this IWhatsAppClient client, UserMessage message, string reply)
        => client.SendAsync(message.To.Id, new
        {
            messaging_product = "whatsapp",
            preview_url = false,
            recipient_type = "individual",
            to = NormalizeNumber(message.From.Number),
            type = "text",
            context = new
            {
                message_id = message.Id
            },
            text = new
            {
                body = reply
            }
        });

    /// <summary>
    /// Replies to a user message with an additional interactive button.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="message">The message to reply to.</param>
    /// <param name="reply">The text message to respond with.</param>
    /// <param name="button">Interactive button for users to reply.</param>
    public static Task ReplyAsync(this IWhatsAppClient client, UserMessage message, string reply, Button button)
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
                        new { type = "reply", reply = new { id = button.Id, title = button.Title } },
                    }
                }
            }
        });

    /// <summary>
    /// Replies to a user message with a additional interactive buttons.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="message">The message to reply to.</param>
    /// <param name="reply">The text message to respond with.</param>
    /// <param name="button1">Interactive button for a user choice.</param>
    /// <param name="button2">Interactive button for a user choice.</param>
    public static Task ReplyAsync(this IWhatsAppClient client, UserMessage message, string reply, Button button1, Button button2)
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
                    }
                }
            }
        });

    /// <summary>
    /// Replies to a user message with a additional interactive buttons.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="message">The message to reply to.</param>
    /// <param name="reply">The text message to respond with.</param>
    /// <param name="button1">Interactive button for a user choice.</param>
    /// <param name="button2">Interactive button for a user choice.</param>
    /// <param name="button3">Interactive button for a user choice.</param>
    public static Task ReplyAsync(this IWhatsAppClient client, UserMessage message, string reply, Button button1, Button button2, Button button3)
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
        });

    /// <summary>
    /// Replies to the message a user reacted to.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="reaction">The reaction from the user.</param>
    /// <param name="reply">The text message to respond with.</param>
    public static Task ReplyAsync(this IWhatsAppClient client, ReactionMessage message, string reply)
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
        });

    /// <summary>
    /// Sends a text message a user given his incoming message, without making it a reply.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="to">The originating user to send a message to.</param>
    /// <param name="message">The text message to send.</param>
    public static Task SendAsync(this IWhatsAppClient client, Message to, string message)
        => SendAsync(client, to.To.Id, to.From.Number, message);

    /// <summary>
    /// Sends a text message a user given his incoming message, without making it a reply.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="to">The originating user to send a message to.</param>
    /// <param name="message">The text message to send.</param>
    /// <param name="button">Interactive button for users to reply.</param>
    public static Task SendAsync(this IWhatsAppClient client, Message to, string message, Button button)
        => SendAsync(client, to.To.Id, to.From.Number, message, button);

    /// <summary>
    /// Sends a text message a user given his incoming message, without making it a reply.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="to">The originating user to send a message to.</param>
    /// <param name="message">The text message to send.</param>
    /// <param name="button1">Interactive button for a user choice.</param>
    /// <param name="button2">Interactive button for a user choice.</param>
    public static Task SendAsync(this IWhatsAppClient client, Message to, string message, Button button1, Button button2)
        => SendAsync(client, to.To.Id, to.From.Number, message, button1, button2);

    /// <summary>
    /// Sends a text message a user given his incoming message, without making it a reply.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="to">The originating user to send a message to.</param>
    /// <param name="message">The text message to send.</param>
    /// <param name="button1">Interactive button for a user choice.</param>
    /// <param name="button2">Interactive button for a user choice.</param>
    /// <param name="button3">Interactive button for a user choice.</param>
    public static Task SendAsync(this IWhatsAppClient client, Message to, string message, Button button1, Button button2, Button button3)
        => SendAsync(client, to.To.Id, to.From.Number, message, button1, button2, button3);

    /// <summary>
    /// Sends a text message a user.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="from">The service number to send the message through.</param>
    /// <param name="to">The user phone number to send the message to.</param>
    /// <param name="message">The text message to send.</param>
    public static Task SendAsync(this IWhatsAppClient client, string from, string to, string message)
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
        });

    /// <summary>
    /// Sends a text message a user.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="from">The service number to send the message through.</param>
    /// <param name="to">The user phone number to send the message to.</param>
    /// <param name="message">The text message to send.</param>
    /// <param name="button">Interactive button for users to reply.</param>
    public static Task SendAsync(this IWhatsAppClient client, string from, string to, string message, Button button)
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
        });

    /// <summary>
    /// Sends a text message a user.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="from">The service number to send the message through.</param>
    /// <param name="to">The user phone number to send the message to.</param>
    /// <param name="message">The text message to send.</param>
    /// <param name="button1">Interactive button for a user choice.</param>
    /// <param name="button2">Interactive button for a user choice.</param>
    public static Task SendAsync(this IWhatsAppClient client, string from, string to, string message, Button button1, Button button2)
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
        });

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
    public static Task SendAsync(this IWhatsAppClient client, string from, string to, string message, Button button1, Button button2, Button button3)
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
        });

    static string NormalizeNumber(string number) =>
        // On the web, we don't get the 9 after 54 \o/
        // so for Argentina numbers, we need to remove the 9.
        number.StartsWith("549", StringComparison.Ordinal) ? "54" + number[3..] : number;
}