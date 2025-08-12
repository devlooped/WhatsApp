using System.Text.Json;
using System.Text.Json.Serialization;
using Devlooped.WhatsApp.Flows;

namespace Devlooped.WhatsApp;

/// <summary>
/// A <see cref="Message"/> containing an interactive reply for either a button or list message.
/// </summary>
/// <param name="Id">The message identifier.</param>
/// <param name="Service">The service that received the message from the Cloud API.</param>
/// <param name="User">The user that sent the message.</param>
/// <param name="Timestamp">Timestamp of the message.</param>
/// <param name="Data">The payload sent by the complete flow action.</param>
public record InteractiveFlowMessage(string Id, Service Service, User User, long Timestamp, JsonElement Data, FlowToken Source) : UserMessage(Id, Service, User, Timestamp)
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override MessageType Type => MessageType.InteractiveFlow;
}