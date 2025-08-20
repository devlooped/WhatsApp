using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Devlooped.WhatsApp.Flows;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OData.Client;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.IO.Pem;

namespace Devlooped.WhatsApp;

public class FlowTests(ITestOutputHelper output)
{
    [Fact]
    public void VerifyFlowRequest()
    {
        var keyPair = GenerateRsaKeyPair();
        var privatePem = ExportPrivatePem(keyPair);

        var aesKey = RandomBytes(16); // 128-bit per docs
        var iv = Convert.FromBase64String("v1U9tB6hUBd4lVDUlaviBg=="); // 16 bytes supported

        using var crypto = new FlowCryptography(privatePem);

        var requestJson = JsonSerializer.Deserialize<JsonElement>(
            """
            {
                "version": "3.0",
                "action": "ping"
            }
            """);

        // Encrypt request payload using same helper as production (unflipped IV for request per spec)
        var plain = Encoding.UTF8.GetBytes(requestJson.GetRawText());
        var cipherWithTag = FlowCryptography.AesGcmEncrypt(aesKey, iv, plain);

        // RSA-encrypt AES key with OAEP SHA-256 (encrypted_aes_key)
        var encryptedKey = RsaOaepSha256Encrypt(keyPair.Public, aesKey);

        var request = new EncryptedFlowData(
            Data: Convert.ToBase64String(cipherWithTag),
            Key: Convert.ToBase64String(encryptedKey),
            IV: Convert.ToBase64String(iv));

        var decrypted = crypto.Decrypt(request);
        Assert.NotNull(decrypted);
        Assert.Equal(aesKey, decrypted.Key);
        Assert.Equal(iv, decrypted.IV);
        output.WriteLine(decrypted.Data.ToString());
        Assert.Equal(requestJson.ToString(), decrypted.Data.ToString());

        var response = new { screen = "SCREEN_NAME", data = new { some_key = "some_value" } };
        var encryptedResponse = crypto.Encrypt(new FlowData(
            JsonSerializer.SerializeToElement(response),
            decrypted.Key,
            decrypted.IV));

        Assert.NotEmpty(encryptedResponse);
    }

    [SecretsTheory("Meta:PrivateKey", "SendFrom", "SendTo")]
    [InlineData("list")]
    [InlineData("data")]
    public async Task SendFlow(string flow)
    {
        var (configuration, client) = Initialize();

        var message = ContentMessage.Create(configuration["SendFrom"]!, configuration["SendTo"]!, "Hello");

        var response = message.CallToAction("Flow Demo", "Show Flow", new FlowParameters(flow)
        {
            Token = Ulid.NewUlid().ToString(),
            Action = FlowAction.DataExchange,
            Mode = FlowMode.Draft
        });

        var sent = await response.SendAsync(client);

        Assert.NotEqual(response, sent);
    }

    [SecretsFact("Meta:PrivateKey", "SendFrom", "SendTo")]
    public async Task SendFlowNavigateData()
    {
        var (configuration, client) = Initialize();

        var message = ContentMessage.Create(configuration["SendFrom"]!, configuration["SendTo"]!, "Hello");

        // Showcases sending an invisible non-whitespace char (https://invisible-characters.com/115F-HANGUL-CHOSEONG-FILLER.html)
        var response = message.CallToAction("ᅟ", "Show Flow", new FlowParameters("data")
        {
            Token = Ulid.NewUlid().ToString(),
            Action = FlowAction.Navigate,
            Mode = FlowMode.Draft,
            Payload = JsonSerializer.SerializeToElement(new
            {
                screen = "welcome_screen",
                data = new
                {
                    agent = "list",
                    service = "5678",
                    user = "pga",
                    flow = "data",
                },
            })
        });

        var sent = await response.SendAsync(client);

        Assert.NotEqual(response, sent);
    }

    [SecretsFact("Meta:PrivateKey", "SendFrom", "SendTo")]
    public async Task SendBlankNavigate()
    {
        var (configuration, client) = Initialize();

        var message = ContentMessage.Create(configuration["SendFrom"]!, configuration["SendTo"]!, "Hello");

        // Showcases sending an invisible non-whitespace char (https://invisible-characters.com/115F-HANGUL-CHOSEONG-FILLER.html)
        var response = message.CallToAction("ᅟ", "Ver/Editar", "simple");
        var sent = await response.SendAsync(client);

        Assert.NotEqual(response, sent);
    }

    [SecretsFact("Meta:PrivateKey")]
    public async Task SendFlowWithData()
    {
        var (configuration, client) = Initialize();

        await client.SendAsync(configuration["SendFrom"]!, new
        {
            messaging_product = "whatsapp",
            recipient_type = "individual",
            to = configuration["SendTo"]!,
            type = "interactive",
            interactive = new
            {
                type = "flow",
                body = new
                {
                    text = "Lista 'Supermercado'"
                },
                action = new
                {
                    name = "flow",
                    parameters = new
                    {
                        flow_cta = "Ver/Editar",
                        flow_message_version = "3",
                        flow_name = "list",
                        mode = "draft",
                        flow_token = "lists",
                        flow_action = "navigate",
                        flow_action_payload = new
                        {
                            screen = "SELECT_LIST",
                            data = new
                            {
                                agent = "list",
                                user = "pga",
                                service = "5678",
                                lists = new[]
                            {
                                new
                                {
                                    id = "supermercado",
                                    main_content = new { title = "Supermercado" },
                                    on_click_action = new
                                    {
                                        name = "navigate",
                                        next = new { type = "screen", name = "SUPERMARKET_SCREEN" },
                                        payload = new { selected_list = "supermercado" }
                                    }
                                },
                                new
                                {
                                    id = "carniceria",
                                    main_content = new { title = "Carnicería" },
                                    on_click_action = new
                                    {
                                        name = "navigate",
                                        next = new { type = "screen", name = "BUTCHER_SCREEN" },
                                        payload = new { selected_list = "carniceria" }
                                    }
                                },
                                new
                                {
                                    id = "ropa",
                                    main_content = new { title = "Ropa" },
                                    on_click_action = new
                                    {
                                        name = "navigate",
                                        next = new { type = "screen", name = "CLOTHING_SCREEN" },
                                        payload = new { selected_list = "ropa" }
                                    }
                                },
                                new
                                {
                                    id = "ferreteria",
                                    main_content = new { title = "Ferretería" },
                                    on_click_action = new
                                    {
                                        name = "navigate",
                                        next = new { type = "screen", name = "HARDWARE_SCREEN" },
                                        payload = new { selected_list = "ferreteria" }
                                    }
                                }
                            },
                                items = new
                                {
                                    supermercado = new[]
                                {
                                    new { id = "leche", title = "Leche entera 1L" },
                                    new { id = "pan", title = "Pan integral" },
                                    new { id = "huevos", title = "Huevos docena" },
                                    new { id = "arroz", title = "Arroz blanco 1kg" },
                                    new { id = "pasta", title = "Pasta spaghetti 500g" },
                                    new { id = "aceite", title = "Aceite de oliva 500ml" },
                                    new { id = "azucar", title = "Azúcar 1kg" },
                                    new { id = "harina", title = "Harina 1kg" },
                                    new { id = "sal", title = "Sal fina 500g" },
                                    new { id = "cafe", title = "Café molido 250g" }
                                },
                                    carniceria = new[]
                                {
                                    new { id = "carne_molida", title = "Carne molida 1kg" },
                                    new { id = "pollo", title = "Pollo entero 2kg" },
                                    new { id = "costilla", title = "Costilla de cerdo 1kg" },
                                    new { id = "filete", title = "Filete de res 500g" },
                                    new { id = "chorizo", title = "Chorizo artesanal 500g" },
                                    new { id = "jamon", title = "Jamón serrano 200g" },
                                    new { id = "salchicha", title = "Salchichas 12 unid" },
                                    new { id = "pechuga", title = "Pechuga de pollo 1kg" }
                                },
                                    ropa = new[]
                                {
                                    new { id = "camiseta", title = "Camiseta blanca M" },
                                    new { id = "pantalon", title = "Pantalón vaquero talla 32" },
                                    new { id = "zapatos", title = "Zapatos deportivos talla 42" },
                                    new { id = "chaqueta", title = "Chaqueta de cuero L" },
                                    new { id = "calcetines", title = "Calcetines pack 6 pares" },
                                    new { id = "cinturon", title = "Cinturón de cuero negro" },
                                    new { id = "sombrero", title = "Sombrero de lana" },
                                    new { id = "bufanda", title = "Bufanda de invierno" }
                                },
                                    ferreteria = new[]
                                {
                                    new { id = "martillo", title = "Martillo de carpintero" },
                                    new { id = "destornillador", title = "Juego de destornilladores 6 piezas" },
                                    new { id = "clavos", title = "Clavos 2 pulgadas 1kg" },
                                    new { id = "tornillos", title = "Tornillos para madera 100 unid" },
                                    new { id = "taladro", title = "Taladro eléctrico 500W" },
                                    new { id = "pintura", title = "Pintura blanca 4L" },
                                    new { id = "brocha", title = "Brocha de pintar 2 pulgadas" },
                                    new { id = "cinta", title = "Cinta métrica 5m" },
                                    new { id = "sierra", title = "Sierra manual" }
                                }
                                }
                            }
                        }
                    }
                }
            }
        });
    }

    [Fact]
    public void DeserializeMessage()
    {
        var data = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText($"Content/WhatsApp/FlowInit.json"), JsonContext.DefaultOptions);
        var json = JsonObject.Create(data, new JsonNodeOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(json);

        json.Add("service", "1234");
        json.Add("user", "5678");

        var message = JsonSerializer.Deserialize<FlowDataRequest>(json, JsonContext.DefaultOptions);

        Assert.NotNull(message);
        Assert.Equal("1234", message.ServiceId);
        Assert.Equal("5678", message.UserNumber);
        Assert.Equal(FlowDataAction.Init, message.Action);
    }

    [Fact]
    public void DeserializeDataMessage()
    {
        var data = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText($"Content/WhatsApp/FlowData.json"));
        var json = JsonObject.Create(data, new JsonNodeOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(json);

        json.Add("service", "1234");
        json.Add("user", "5678");

        var manual = JsonSerializer.Serialize(new FlowDataRequest("1234", "5678", FlowDataAction.DataExchange, "Welcome",
            JsonSerializer.SerializeToElement(new JsonObject { ["foo"] = "bar" }),
            FlowToken.Decode("agent:list;service:1234;user:5678;flow:data;token:asdf1234")), JsonContext.DefaultOptions);

        var message = JsonSerializer.Deserialize<FlowDataRequest>(json, JsonContext.DefaultOptions);

        Assert.NotNull(message);
        Assert.Equal("1234", message.ServiceId);
        Assert.Equal("5678", message.UserNumber);
        Assert.Equal(FlowDataAction.DataExchange, message.Action);
        Assert.Equal("Welcome", message.Screen);
        Assert.Equal("bar", message.Data.GetProperty("foo").GetString());
    }

    static AsymmetricCipherKeyPair GenerateRsaKeyPair()
    {
        var keyGen = new RsaKeyPairGenerator();
        keyGen.Init(new KeyGenerationParameters(new SecureRandom(), 2048));
        return keyGen.GenerateKeyPair();
    }

    static string ExportPrivatePem(AsymmetricCipherKeyPair keyPair)
    {
        var privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(keyPair.Private);
        using var sw = new StringWriter();
        var pemWriter = new PemWriter(sw);
        pemWriter.WriteObject(new PemObject("PRIVATE KEY", privateKeyInfo.GetEncoded()));
        pemWriter.Writer.Flush();
        return sw.ToString();
    }

    static byte[] RandomBytes(int length)
    {
        var bytes = new byte[length];
        new SecureRandom().NextBytes(bytes);
        return bytes;
    }

    static byte[] RsaOaepSha256Encrypt(AsymmetricKeyParameter publicKey, byte[] data)
    {
        var oaep = new OaepEncoding(new RsaEngine(), new Sha256Digest(), new Sha256Digest(), null);
        oaep.Init(true, publicKey);
        return oaep.ProcessBlock(data, 0, data.Length);
    }

    (IConfiguration configuration, WhatsAppClient client) Initialize()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<WhatsAppClientTests>()
            .Build();

        var collection = new ServiceCollection()
            .AddSingleton<ILoggerFactory>(new MockLogger(output))
            .AddHttpClient()
            .AddSingleton<IConfiguration>(configuration);

        collection.AddOptions<MetaOptions>()
            .BindConfiguration("Meta")
            .ValidateDataAnnotations();

        collection.AddSingleton<WhatsAppClient>();

        var services = collection.BuildServiceProvider();
        return (configuration, services.GetRequiredService<WhatsAppClient>());
    }
}
