using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Devlooped.WhatsApp;

public class FlowTests(ITestOutputHelper output)
{
    [SecretsFact("Meta:PrivateKey", "Meta:PublicKey")]
    public void VerifyFlowRequest()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<FlowTests>()
            .Build();

        var crypto = new FlowCryptography(configuration["Meta:PrivateKey"]!);

        var request = new EncryptedFlowData(
            "0zMVQ5xXwGZ8nViXojCYovFRvrTB3dx2bDA4AhXoPhFirbNlsN9Gi7JYDDoBZ44W6g==",
            @"e5JGMhHduIeaynRKPzleeZdcybczOJnTbLZ0nB0wWLYak1IkNbb06ZDNKt29h9A7wCOAJnf3DaWzWR5365z70QMgtN5oZRWkVEJgzNtIsM7vgbT2TZtVTLXuSNQrS4ueqF7s/d6WKLqhdz3+Ab2kebJlFoDbXQxMqVI2HK8qd5jI0lPIALp28tORq+Z3etz3qYW8p1K4ruc77LqYHrdF1YePLES+c5F90WQMt7gtbJMCMoQFPhViKXVOykJ0gChvqCxfu2wH/L0vU9HdhOFK2rZPxq123BvmLCLwSFt+CnQY64iambrTZXz4Z+GhtSCR9O8MBck6mDl9eWT/RAkxbg==",
            "v1U9tB6hUBd4lVDUlaviBg==");

        var decrypted = crypto.Decrypt(request);
        Assert.NotNull(decrypted);
        output.WriteLine(decrypted.Data.ToString());

        var response = new { screen = "SCREEN_NAME", data = new { some_key = "some_value" } };
        var encrypted = crypto.Encrypt(new FlowData(
            JsonSerializer.SerializeToElement(response),
            decrypted.Key,
            decrypted.IV));

        Assert.NotEmpty(encrypted);
    }
}
