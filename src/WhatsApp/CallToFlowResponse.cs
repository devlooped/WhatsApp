using System.Text.Json;
using System.Text.Json.Serialization;
using Devlooped.WhatsApp.Flows;

namespace Devlooped.WhatsApp;

/// <summary>
/// Represents an interactive call to initiate a flow that can be sent in response to a user message.
/// </summary>
/// <see cref="https://developers.facebook.com/docs/whatsapp/cloud-api/messages/interactive-flow-messages"/>
/// <param name="ServiceId">The identifier of the service handling the message.</param>
/// <param name="UserId">The phone number of the recipient in international format.</param>
/// <param name="Text">The content of the message calling to initiate the flow.</param>
/// <param name="Action">The action button text.</param>
public record CallToFlowResponse : Response
{
    /// <summary>Initializes a new instance of the <see cref="CallToFlowResponse"/> record.</summary>
    [JsonConstructor]
    public CallToFlowResponse(string serviceId, string userId, string text, string action, FlowParameters flow) : base(serviceId, userId)
    {
        Text = text;
        Action = action;
        Flow = flow;
    }

    /// <summary>Initializes a new instance of the <see cref="CallToFlowResponse"/> record using an existing message.</summary>
    public CallToFlowResponse(IMessage message, string text, string action, long flowId)
        : this(message.ServiceId, message.UserId, text, action, new FlowParameters(flowId))
    { }

    /// <summary>Initializes a new instance of the <see cref="CallToFlowResponse"/> record using an existing message.</summary>
    public CallToFlowResponse(IMessage message, string text, string action, string flowName)
        : this(message.ServiceId, message.UserId, text, action, new FlowParameters(flowName))
    { }

    /// <summary>The text message that prompts the user to initiate the flow via the <see cref="Action"/> button.</summary>
    public string Text { get; }

    /// <summary>The call to action button text.</summary>
    public string Action { get; }

    /// <summary>Additional parameters for the flow to be initiated.</summary>
    public FlowParameters Flow { get; init; }

    protected override async Task<string?> SendCoreAsync(IWhatsAppClient client, CancellationToken cancellation = default)
    {
        if (Flow.Action == FlowAction.DataExchange && Flow.Payload != null)
            throw new NotSupportedException("Payload data can only be provided for Navigate flow action.");

        var token = FlowToken.Encode(this);

        object parameters = Flow.Id.HasValue ?
            new
            {
                flow_message_version = "3",
                flow_cta = Action,
                flow_id = Flow.Id,
                mode = Flow.Mode.ToString().ToLowerInvariant(),
                flow_token = token,
                flow_action = Flow.Action == FlowAction.DataExchange ? "data_exchange" : "navigate",
                flow_action_payload = Flow.Payload
            } :
            new
            {
                flow_message_version = "3",
                flow_cta = Action,
                flow_name = Flow.Name,
                mode = Flow.Mode.ToString().ToLowerInvariant(),
                flow_token = token,
                flow_action = Flow.Action == FlowAction.DataExchange ? "data_exchange" : "navigate",
                flow_action_payload = Flow.Payload
            };

        var id = await client.SendAsync(ServiceId, new
        {
            messaging_product = "whatsapp",
            recipient_type = User.IsBusinessScopedUserId(UserId) ? "business_scoped_user_id" : "individual",
            to = UserId,
            type = "interactive",
            interactive = new
            {
                type = "flow",
                body = new
                {
                    text = Text
                },
                action = new
                {
                    name = "flow",
                    parameters
                }
            }
        });

        return id;
    }
}

/// <summary>Parameters for initiating or continuing a flow.</summary>
public record FlowParameters
{
    [JsonConstructor]
    internal FlowParameters(long? id = default, string? name = default) => (Id, Name) = (id, name);

    /// <summary>Initializes a new instance of the <see cref="FlowParameters"/> record using a flow identifier.</summary>
    public FlowParameters(long flowId) => Id = flowId;

    /// <summary>Initializes a new instance of the <see cref="FlowParameters"/> record using a flow name.</summary>
    public FlowParameters(string flowName) => Name = flowName;

    /// <summary>Gets the flow identifier, if the instance was constructed with a number.</summary>
    public long? Id { get; }

    /// <summary>Gets the flow name, if the instance was constructed with a string.</summary>
    public string? Name { get; }

    /// <summary>Indicates the action to perform when the flow is initiated. Defaults to <see cref="FlowAction.Navigate"/>.</summary>
    public FlowAction Action { get; set; } = FlowAction.Navigate;

    /// <summary>Indicates the mode of the flow, either draft or published.</summary>
    public FlowMode Mode { get; set; } = FlowMode.Published;

    /// <summary>Optional data payload for the flow, only valid when <see cref="FlowAction"/> is <see cref="FlowAction.Navigate"/>.</summary>
    public JsonElement? Payload { get; set; }

    /// <summary>Optional token to continue the flow, used for resuming or continuing a flow session.</summary>
    public string? Token { get; set; }
}
