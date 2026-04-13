using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Devlooped.WhatsApp;

/// <summary>
/// Configuration for a WhatsApp Business Account (WABA).
/// </summary>
public class AccountOptions
{
    /// <summary>Access token for this account, used for both messaging and Flows management API calls.</summary>
    public required string AccessToken { get; set; }

    /// <summary>Verify token used to register this account's webhook with Meta.</summary>
    public required string VerifyToken { get; set; }

    /// <summary>Optional RSA private key (PEM) used to decrypt Flows endpoint data exchange requests.</summary>
    public string? PrivateKey { get; set; }

    /// <summary>List of phone number IDs associated with this account. All share the account token.</summary>
    public string[] Numbers { get; set; } = [];
}

/// <summary>
/// Options for handling communication with WhatsApp for Business from Meta.
/// </summary>
public class MetaOptions
{
    /// <summary>API version for messages, defaults to v22.0.</summary>
    [DefaultValue("v22.0")]
    public string ApiVersion { get; set; } = "v22.0";

    /// <summary>
    /// WhatsApp Business Accounts indexed by account ID. Each account holds its access token, 
    /// verify token, an optional RSA private key for Flows, and the list of phone number IDs it owns.
    /// </summary>
    [MinLength(1, ErrorMessage = "At least one account must be configured, e.g. Meta:Accounts:12345:AccessToken=asdf")]
    public IDictionary<string, AccountOptions> Accounts { get; set; } = new Dictionary<string, AccountOptions>();

    /// <summary>
    /// Returns the access token for the given account ID.
    /// Returns <see langword="null"/> if no account is found with that ID.
    /// </summary>
    public string? GetAccountToken(string accountId)
        => Accounts.TryGetValue(accountId, out var account) ? account.AccessToken : null;

    /// <summary>
    /// Returns the verify token for the given account ID.
    /// Returns <see langword="null"/> if no account is found with that ID.
    /// </summary>
    public string? GetVerifyToken(string accountId)
        => Accounts.TryGetValue(accountId, out var account) ? account.VerifyToken : null;

    /// <summary>
    /// Returns the account ID whose <see cref="AccountOptions.VerifyToken"/> matches <paramref name="verifyToken"/>,
    /// or <see langword="null"/> if no matching account is found.
    /// </summary>
    public string? FindAccountByVerifyToken(string verifyToken)
        => Accounts.FirstOrDefault(a => a.Value.VerifyToken == verifyToken).Key;

    /// <summary>
    /// Returns the access token for the given phone number ID by finding which account owns it.
    /// Returns <see langword="null"/> if no account contains <paramref name="numberId"/>.
    /// </summary>
    public string? GetToken(string numberId)
    {
        foreach (var account in Accounts.Values)
        {
            if (Array.IndexOf(account.Numbers, numberId) >= 0)
                return account.AccessToken;
        }

        return null;
    }

    /// <summary>
    /// Returns the RSA private key (PEM) for the given account ID.
    /// Returns <see langword="null"/> if the account is not found or has no private key configured.
    /// </summary>
    public string? GetPrivateKey(string accountId)
        => Accounts.TryGetValue(accountId, out var account) ? account.PrivateKey : null;

    /// <summary>
    /// Returns all non-null RSA private keys (PEM) across all accounts, for use when the
    /// account ID is not known at decryption time.
    /// </summary>
    public IEnumerable<string> GetPrivateKeys()
        => Accounts.Values.Select(a => a.PrivateKey).Where(k => k is not null)!;
}
