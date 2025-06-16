using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

namespace Devlooped.WhatsApp;

/// <summary>
/// Provides functionality to evaluate whether a feature is enabled based on the availability of specific services. 
/// </summary>
/// <remarks>This filter determines feature availability by checking the presence of required services in the
/// application's dependency injection container. It supports evaluating features such as <see
/// cref="FeatureFlags.Storage"/> and <see cref="FeatureFlags.Conversation"/>.</remarks>
/// <param name="serviceProvider"></param>
[FilterAlias(Alias)]
public class FeatureFilter(IServiceProvider serviceProvider) : IFeatureFilter
{
    public const string Alias = "whatsapp_features";

    public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
    {
        var result = default(bool);

        switch (context.FeatureName)
        {
            case nameof(FeatureFlags.Storage): result = serviceProvider.GetService<IStorageService>() != null; break;
            case nameof(FeatureFlags.Conversation): result = serviceProvider.GetService<IConversationService>() != null; break;
        }

        return Task.FromResult(result);
    }
}

static class FeatureFilterExtensions
{
    public static void AddFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        var config = new ConfigurationBuilder()
            .AddConfiguration(configuration)
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                    { $"FeatureManagement:{FeatureFlags.Storage}:EnabledFor:0:Name", FeatureFilter.Alias },
                    { $"FeatureManagement:{FeatureFlags.Conversation}:EnabledFor:0:Name", FeatureFilter.Alias }
            })
            .Build();

        services.AddFeatureManagement(config).AddFeatureFilter<FeatureFilter>();
    }
}