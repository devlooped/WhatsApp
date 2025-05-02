using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Devlooped.WhatsApp;

/// <summary>
/// Options for handling communication with WhatsApp for Business from Meta.
/// </summary>
public class MetaOptions
{
    /// <summary>
    /// API version for messages, defaults to v22.0.
    /// </summary>
    [DefaultValue("v22.0")]
    public string ApiVersion { get; set; } = "v22.0";

    /// <summary>
    /// Custom string used in the Meta App Dashboard for configuring the webhook.
    /// </summary>
    [Required(ErrorMessage = "Meta:VerifyToken is required to properly register with WhatsApp for Business webhooks.")]
    public required string VerifyToken { get; set; }

    /// <summary>
    /// Contains pairs of number ID > access token for WhatsApp for Business phone numbers.
    /// </summary>
    [MinLength(1, ErrorMessage = "At least one number ID > access token pair is required, i.e. Meta:12345=asdf.")]
    public IDictionary<string, string> Numbers { get; set; } = new Dictionary<string, string>();
}
