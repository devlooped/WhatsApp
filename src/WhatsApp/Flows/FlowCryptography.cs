using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace Devlooped.WhatsApp.Flows;

/// <summary>Represents an encrypted flow message exchanged with WhatsApp Business API.</summary>
record EncryptedFlowData(
    [property: JsonPropertyName("encrypted_flow_data")] string Data,
    [property: JsonPropertyName("encrypted_aes_key")] string Key,
    [property: JsonPropertyName("initial_vector")] string IV);

/// <summary>Parsed flow data containing decrypted JSON and AES key/IV.</summary>
record FlowData<TData>(TData Data, byte[] Key, byte[] IV);

/// <summary>Represents flow data with decrypted JSON content, AES key, and IV.</summary>
record FlowData(JsonElement Data, byte[] Key, byte[] IV) : FlowData<JsonElement>(Data, Key, IV)
{
    /// <summary>Creates a new instance of <see cref="FlowData{TData}"/> with the specified data.</summary>
    public FlowData<TData> With<TData>(TData data) =>
        new(data, Key, IV);
}

/// <summary>Implements the flow message encryption and decryption for the WhatsApp Business API.</summary>
class FlowCryptography : IDisposable
{
    const int TagLengthBytes = 16;
    const int StandardNonceLength = 12;

    readonly RSA rsa;

    /// <summary>Initializes the class with the provided RSA private key in PEM format.</summary>
    public FlowCryptography(string privatePem)
    {
        rsa = RSA.Create();
        rsa.ImportFromPem(Throw.IfNullOrEmpty(privatePem));
    }

    /// <summary>Initializes the class with the provided RSA private key in PEM format and a passphrase for decryption.</summary>
    public FlowCryptography(string privatePem, string passphrase)
    {
        rsa = RSA.Create();
        rsa.ImportFromEncryptedPem(Throw.IfNullOrEmpty(privatePem), passphrase);
    }

    /// <summary>Decrypts the provided encrypted flow data into a <see cref="FlowData"/> object.</summary>
    public FlowData Decrypt(EncryptedFlowData data)
    {
        // Inline decode & decrypt pipeline (Base64 -> RSA -> AES-GCM -> JSON)
        var aesKey = rsa.Decrypt(Convert.FromBase64String(data.Key), RSAEncryptionPadding.OaepSHA256);
        var iv = Convert.FromBase64String(data.IV);
        var cipher = Convert.FromBase64String(data.Data);
        var plaintext = AesGcmDecrypt(aesKey, iv, cipher);
        var json = JsonSerializer.Deserialize<JsonElement>(Encoding.UTF8.GetString(plaintext));
        return new FlowData(json, aesKey, iv);
    }

    /// <summary>Decrypts the provided encrypted flow data into a <see cref="FlowData"/> object, returning false on failure.</summary>
    public bool TryDecrypt(EncryptedFlowData data, out FlowData? result)
    {
        try
        {
            result = Decrypt(data);
            return true;
        }
        catch (CryptographicException)
        {
            result = null;
            return false;
        }
    }

    /// <summary>Encrypts the provided flow data into a Base64-encoded string.</summary>
    public string Encrypt<TData>(FlowData<TData> data)
    {
        // Derive nonce via bit-flip (encapsulated) and serialize JSON directly to UTF-8 bytes.
        var flippedIv = FlipIvBits(data.IV);
        var payload = JsonSerializer.SerializeToUtf8Bytes(data.Data);
        var cipherWithTag = AesGcmEncrypt(data.Key, flippedIv, payload);
        return Convert.ToBase64String(cipherWithTag);
    }

    /// <summary>Disposes the inner RSA key.</summary>
    public void Dispose() => rsa.Dispose();

    // Single decryption attempt with provided nonce.
    static byte[] DecryptOnce(byte[] key, byte[] nonce, byte[] input)
    {
        var gcm = new GcmBlockCipher(new Org.BouncyCastle.Crypto.Engines.AesEngine());
        var parameters = new AeadParameters(new KeyParameter(key), TagLengthBytes * 8, nonce);
        gcm.Init(false, parameters);
        var plain = new byte[gcm.GetOutputSize(input.Length)];
        var len = gcm.ProcessBytes(input, 0, input.Length, plain, 0);
        len += gcm.DoFinal(plain, len);
        if (len != plain.Length)
            Array.Resize(ref plain, len);
        return plain;
    }

    // Tries full IV first; on auth failure retries with truncated 12-byte nonce for backward compatibility.
    static byte[] AesGcmDecrypt(byte[] key, byte[] iv, byte[] input)
    {
        try
        {
            return DecryptOnce(key, iv, input);
        }
        catch (InvalidCipherTextException) when (iv.Length >= StandardNonceLength)
        {
            var truncated = new byte[StandardNonceLength];
            Array.Copy(iv, 0, truncated, 0, StandardNonceLength);
            return DecryptOnce(key, truncated, input);
        }
    }

    // Made internal so tests can reuse the exact implementation for client-side payload generation without duplicating logic.
    internal static byte[] AesGcmEncrypt(byte[] key, byte[] iv, byte[] plain)
    {
        if (iv.Length < StandardNonceLength)
            throw new ArgumentException("IV must be at least 12 bytes.");

        var gcm = new GcmBlockCipher(new Org.BouncyCastle.Crypto.Engines.AesEngine());
        var parameters = new AeadParameters(new KeyParameter(key), TagLengthBytes * 8, iv);
        gcm.Init(true, parameters);
        var cipher = new byte[gcm.GetOutputSize(plain.Length)];
        var len = gcm.ProcessBytes(plain, 0, plain.Length, cipher, 0);
        len += gcm.DoFinal(cipher, len);
        if (len != cipher.Length)
            Array.Resize(ref cipher, len);
        return cipher;
    }

    // Encapsulated IV bit-flip transformation used for nonce derivation during encryption.
    static byte[] FlipIvBits(byte[] iv)
    {
        var flipped = new byte[iv.Length];
        for (var i = 0; i < iv.Length; i++)
            flipped[i] = (byte)~iv[i];
        return flipped;
    }
}