using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.AI;

namespace Devlooped.WhatsApp;

/// <summary>
/// Usability extensions for creating responses for user messages.
/// </summary>
public static partial class MessageExtensions
{
    extension(IMessage message)
    {
        /// <summary>
        /// Gets or sets whether the message was sent from the WhatsApp CLI.
        /// </summary>
        public bool FromConsole
        {
            get => (message.AdditionalProperties ??= []).TryGetValue("FromConsole", out var value) ? value as bool? ?? default : default;
            set => (message.AdditionalProperties ??= [])["FromConsole"] = value;
        }

        /// <summary>
        /// Alternative text to send to the console, if the message is intended to reach the CLI.
        /// </summary>
        public string? ConsoleText
        {
            get => (message.AdditionalProperties ??= []).TryGetValue("ConsoleText", out var value) ? value as string : null;
            set => (message.AdditionalProperties ??= [])["ConsoleText"] = value;
        }

        /// <summary>
        /// The message is intended for consumption only in the console and should not be sent to WhatsApp.
        /// </summary>
        public bool ConsoleOnly
        {
            get => (message.AdditionalProperties ??= []).TryGetValue("ConsoleOnly", out var value) ? value as bool? ?? default : default;
            set => (message.AdditionalProperties ??= [])["ConsoleOnly"] = value;
        }
    }

    extension<T>(T message) where T : IMessage
    {
        /// <summary>
        /// Configures the response by applying the specified action to it.
        /// </summary>
        public T Configure(Action<T> configure)
        {
            configure(message);
            return message;
        }
    }

    extension(Message message)
    {
        /// <summary>
        /// Configures the message by applying the specified action to it.
        /// </summary>
        public Message With(Action<AdditionalPropertiesDictionary> properties)
        {
            if (message.AdditionalProperties is null)
                message = message with { AdditionalProperties = [] };

            properties(message.AdditionalProperties);
            return message;
        }
    }

    extension<T>(T response) where T : Response
    {
        /// <summary>
        /// Sets additional properties in the response.
        /// </summary>
        public T With(Action<AdditionalPropertiesDictionary> properties)
        {
            if (response.AdditionalProperties is null)
                response = response with { AdditionalProperties = [] };

            properties(response.AdditionalProperties);
            return response;
        }

        /// <summary>
        /// Marks the response for console use only by setting the <c>ConsoleOnly</c> property to <see langword="true"/>.
        /// </summary>
        public T ForConsoleOnly()
        {
            response.ConsoleOnly = true;
            return response;
        }

        /// <summary>
        /// Sets the console alternate text for the response, in case the response is intended to be sent to the CLI.
        /// </summary>
        public T WithConsoleText(string text)
        {
            response.ConsoleText = text;
            return response;
        }
    }

    /// <summary>
    /// Creates a reaction response for the user message.
    /// </summary>
    public static ReactionResponse React(this IMessage message, string emoji)
        => message is UserMessage user
            ? new(user.Service, message.UserNumber, message.Id, message.ConversationId, emoji)
            : new(message.ServiceId, message.UserNumber, message.Id, message.ConversationId, emoji);

    /// <summary>
    /// Creates a simple template response for the message.
    /// </summary>
    public static TemplateResponse Template(this IMessage message, string name, string language)
        => new(message.ServiceId, message.UserNumber, message.Id, message.ConversationId, name, language);

    /// <summary>
    /// Creates a complex template response for the message.
    /// </summary>
    /// <param name="message">The message to respond to.</param>
    /// <param name="template">The full template object as supported by the WhatsApp for Business API.</param>
    /// <see cref="https://developers.facebook.com/docs/whatsapp/api/messages/message-templates#supported-languages"/>
    /// <see cref="https://developers.facebook.com/docs/whatsapp/cloud-api/reference/messages/#template-object"/>
    /// <see cref="https://developers.facebook.com/docs/whatsapp/cloud-api/reference/messages/#components-object"/>
    public static TemplateResponse Template(this IMessage message, object template)
        => new(message.ServiceId, message.UserNumber, message.Id, message.ConversationId, template);

    /// <summary>
    /// Sends a typing indicator status to signal that there is an ongoing response to the user message.
    /// </summary>
    public static TypingResponse Typing(this UserMessage message)
        => new(message.Service, message.User.Number, message.Id, message.ConversationId);

    /// <summary>
    /// Creates a text response for the message.
    /// </summary>
    public static TextResponse Reply(this IMessage message, string text)
        => message is UserMessage user
        ? new(user.Service, message.UserNumber, message.Id, message.ConversationId, text)
        : new(message.ServiceId, message.UserNumber, message.Id, message.ConversationId, text);

    /// <summary>
    /// Creates a text response with buttons for the message.
    /// </summary>
    public static TextResponse Reply(this IMessage message, string text, Button button1, Button? button2 = default)
        => message is UserMessage user
            ? new(user.Service, message.UserNumber, message.Id, message.ConversationId, text, button1, button2)
            : new(message.ServiceId, message.UserNumber, message.Id, message.ConversationId, text, button1, button2);

    /// <summary>
    /// Attempts to retrieve a single message from the specified collection.
    /// </summary>
    /// <remarks>This method checks whether the provided collection contains exactly one message. If so, the
    /// message is assigned to the <paramref name="message"/> parameter. If the collection is empty, contains more than
    /// one message, or is null, the method returns <see langword="false"/> and <paramref name="message"/> is set to
    /// <see langword="null"/>.</remarks>
    /// <param name="messages">The collection of messages to evaluate. Must not be null.</param>
    /// <param name="message">When this method returns <see langword="true"/>, contains the single message from the collection. When this
    /// method returns <see langword="false"/>, contains <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the collection contains exactly one message; otherwise, <see langword="false"/>.</returns>
    internal static bool TrySingle(this IEnumerable<IMessage> messages, [NotNullWhen(true)] out IMessage? message)
    {
        if (messages is IList<IMessage> list && list.Count == 1)
        {
            message = list[0];
        }
        else if (messages is IMessage[] array && array.Length == 1)
        {
            message = array[0];
        }
        else
        {
            message = null;
        }

        return message != null;
    }
}
