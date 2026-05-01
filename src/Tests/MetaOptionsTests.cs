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
                { "Meta:Accounts:1234567890:VerifyToken", "test-challenge" },
                { "Meta:Accounts:1234567890:AccessToken", "test-access-token" },
                { "Meta:Accounts:1234567890:Numbers:0", "1234567890" }
            }).Build());

        collection.ConfigureCoreOptions();

        var options = collection
            .BuildServiceProvider()
            .GetRequiredService<IOptions<MetaOptions>>().Value;

        Assert.NotNull(options);
        Assert.Equal("test-challenge", options.GetVerifyToken("1234567890"));
        Assert.Equal("test-access-token", options.GetToken("1234567890"));
        Assert.Equal("v22.0", options.ApiVersion);
    }

    [Fact]
    public void SingleStringNumberBindsAsArray()
    {
        var collection = new ServiceCollection()
            .AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()
            {
                { "Meta:Accounts:1234567890:VerifyToken", "test-challenge" },
                { "Meta:Accounts:1234567890:AccessToken", "test-access-token" },
                { "Meta:Accounts:1234567890:Numbers", "9876543210" }
            }).Build());

        collection.ConfigureCoreOptions();

        var options = collection
            .BuildServiceProvider()
            .GetRequiredService<IOptions<MetaOptions>>().Value;

        Assert.NotNull(options);
        Assert.Single(options.Accounts["1234567890"].Numbers);
        Assert.Equal("9876543210", options.Accounts["1234567890"].Numbers[0]);
        Assert.Equal("test-access-token", options.GetToken("9876543210"));
    }

    [Fact]
    public void CommaSeparatedNumbersStringBindsAsArray()
    {
        var collection = new ServiceCollection()
            .AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()
            {
                { "Meta:Accounts:1234567890:VerifyToken", "test-challenge" },
                { "Meta:Accounts:1234567890:AccessToken", "test-access-token" },
                { "Meta:Accounts:1234567890:Numbers", "9876543210, 1111111111" }
            }).Build());

        collection.ConfigureCoreOptions();

        var options = collection
            .BuildServiceProvider()
            .GetRequiredService<IOptions<MetaOptions>>().Value;

        Assert.NotNull(options);
        Assert.Equal(2, options.Accounts["1234567890"].Numbers.Length);
        Assert.Equal("test-access-token", options.GetToken("9876543210"));
        Assert.Equal("test-access-token", options.GetToken("1111111111"));
    }

    [Fact]
    public void FindAccountByVerifyTokenReturnsNullForUnknownToken()
    {
        var options = new MetaOptions
        {
            Accounts = new Dictionary<string, AccountOptions>
            {
                ["1234"] = new AccountOptions { AccessToken = "tok", VerifyToken = "known-token", Numbers = ["9876"] }
            }
        };

        Assert.Null(options.FindAccountByVerifyToken("unknown-token"));
        Assert.Equal("1234", options.FindAccountByVerifyToken("known-token"));
    }

    [Fact]
    public void FailsWithoutAccounts()
    {
        var collection = new ServiceCollection()
            .AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()
            {
            }).Build());

        collection.ConfigureCoreOptions();

        var ex = Assert.Throws<OptionsValidationException>(() => collection
            .BuildServiceProvider()
            .GetRequiredService<IOptions<MetaOptions>>().Value);

        Assert.Single(ex.Failures);
        Assert.Contains(nameof(MetaOptions), ex.Failures.First());
        Assert.Contains(nameof(MetaOptions.Accounts), ex.Failures.First());
    }

    [Fact]
    public void GetTokenReturnsNullForUnknownNumber()
    {
        var options = new MetaOptions
        {
            Accounts = new Dictionary<string, AccountOptions>
            {
                ["1234"] = new AccountOptions { AccessToken = "tok", VerifyToken = "vt", Numbers = ["9876"] }
            }
        };

        Assert.Null(options.GetToken("0000"));
        Assert.Equal("tok", options.GetToken("9876"));
    }

}
