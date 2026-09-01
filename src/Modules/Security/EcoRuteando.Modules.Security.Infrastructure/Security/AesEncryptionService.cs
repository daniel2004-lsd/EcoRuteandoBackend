using System.Security.Cryptography;
using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using Microsoft.Extensions.Configuration;

namespace EcoRuteando.Modules.Security.Infrastructure.Security;

public sealed class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public AesEncryptionService(IConfiguration configuration)
    {
        var base64Key = configuration["Encryption:TotpSecretKey"]
            ?? throw new InvalidOperationException(
                "Missing configuration 'Encryption:TotpSecretKey'.");

        _key = Convert.FromBase64String(base64Key);

        if (_key.Length != 32)
        {
            throw new InvalidOperationException(
                "Encryption:TotpSecretKey must decode to 32 bytes (AES-256).");
        }
    }

    public byte[] Encrypt(byte[] plainBytes)
    {
        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize];
        RandomNumberGenerator.Fill(nonce);

        var tag = new byte[AesGcm.TagByteSizes.MaxSize];
        var ciphertext = new byte[plainBytes.Length];

        using var aes = new AesGcm(_key, tag.Length);
        aes.Encrypt(nonce, plainBytes, ciphertext, tag);

        var result = new byte[nonce.Length + tag.Length + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, result, nonce.Length, tag.Length);
        Buffer.BlockCopy(ciphertext, 0, result, nonce.Length + tag.Length, ciphertext.Length);

        return result;
    }

    public byte[] Decrypt(byte[] cipherBytes)
    {
        const int nonceSize = 12;
        const int tagSize = 16;

        var nonce = cipherBytes.AsSpan(0, nonceSize);
        var tag = cipherBytes.AsSpan(nonceSize, tagSize);
        var ciphertext = cipherBytes.AsSpan(nonceSize + tagSize);

        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(_key, tagSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        return plaintext;
    }
}
