using Microsoft.FeatureManagement;

namespace Devlooped.WhatsApp;

/// <summary>
/// Represents the available feature flags that can be enabled or disabled in the application.
/// </summary>
/// <remarks>Feature flags are used to control the availability of specific features within the application. Use
/// this enumeration to specify which features are being targeted for configuration or runtime checks.</remarks>
public enum FeatureFlags
{
    Storage,
    Conversation
}

static class FeatureManagerExtensions
{
    public static Task<bool> IsEnabledAsync(this IFeatureManager featureManager, FeatureFlags feature)
        => featureManager.IsEnabledAsync(feature.ToString());
}