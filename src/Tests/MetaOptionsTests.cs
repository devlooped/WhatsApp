using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Devlooped.WhatsApp;

public class MetaOptionsTests
{
    [Fact]
    public void ValidateOptions()
    {
        var collection = new ServiceCollection()
            .AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()
            {
                { "Meta:VerifyToken", "test-challenge" },
                { "Meta:Numbers:1234567890", "test-access-token" }
            }).Build());

        collection.AddOptions<MetaOptions>()
            .BindConfiguration("Meta")
            .ValidateDataAnnotations();

        var options = collection
            .BuildServiceProvider()
            .GetRequiredService<IOptions<MetaOptions>>().Value;

        Assert.NotNull(options);
        Assert.Equal("test-challenge", options.VerifyToken);
        Assert.Equal("test-access-token", options.Numbers["1234567890"]);
        Assert.Equal("v22.0", options.ApiVersion);
    }

    [Fact]
    public void FailsWithoutChallenge()
    {
        var collection = new ServiceCollection()
            .AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()
            {
                { "Meta:Numbers:1234567890", "test-access-token" }
            }).Build());

        collection.AddOptions<MetaOptions>()
            .BindConfiguration("Meta")
            .ValidateDataAnnotations();

        var ex = Assert.Throws<OptionsValidationException>(() => collection
            .BuildServiceProvider()
            .GetRequiredService<IOptions<MetaOptions>>().Value);

        Assert.Single(ex.Failures);
        Assert.Contains(nameof(MetaOptions), ex.Failures.First());
        Assert.Contains(nameof(MetaOptions.VerifyToken), ex.Failures.First());
    }

    [Fact]
    public void FailsWithoutNumbers()
    {
        var collection = new ServiceCollection()
            .AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()
            {
                { "Meta:VerifyToken", "test-challenge" },
            }).Build());

        collection.AddOptions<MetaOptions>()
            .BindConfiguration("Meta")
            .ValidateDataAnnotations();

        var ex = Assert.Throws<OptionsValidationException>(() => collection
            .BuildServiceProvider()
            .GetRequiredService<IOptions<MetaOptions>>().Value);

        Assert.Single(ex.Failures);
        Assert.Contains(nameof(MetaOptions), ex.Failures.First());
        Assert.Contains(nameof(MetaOptions.Numbers), ex.Failures.First());
    }

}
