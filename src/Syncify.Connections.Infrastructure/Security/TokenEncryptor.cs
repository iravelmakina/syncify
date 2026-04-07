using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Syncify.Connections.Application.Ports;

namespace Syncify.Connections.Infrastructure.Security;

internal sealed class TokenEncryptor : ITokenEncryptor
{
    private readonly byte[] _key;

    public TokenEncryptor(IOptions<EncryptionOptions> options)
    {
        var keyString = options.Value.Key;

        if (string.IsNullOrWhiteSpace(keyString))
            throw new InvalidOperationException("Encryption:Key is not configured.");

        _key = Convert.FromBase64String(keyString);

        if (_key.Length != 32)
            throw new InvalidOperationException("Encryption key must be 256 bits (32 bytes).");
    }

    public string Encrypt(string plaintext)
    {
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize];
        RandomNumberGenerator.Fill(nonce);

        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];

        using var aes = new AesGcm(_key, tag.Length);
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        var result = new byte[nonce.Length + ciphertext.Length + tag.Length];
        nonce.CopyTo(result, 0);
        ciphertext.CopyTo(result, nonce.Length);
        tag.CopyTo(result, nonce.Length + ciphertext.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string encryptedText)
    {
        var combined = Convert.FromBase64String(encryptedText);

        var nonceSize = AesGcm.NonceByteSizes.MaxSize;
        var tagSize = AesGcm.TagByteSizes.MaxSize;

        var nonce = combined.AsSpan(0, nonceSize);
        var ciphertext = combined.AsSpan(nonceSize, combined.Length - nonceSize - tagSize);
        var tag = combined.AsSpan(combined.Length - tagSize, tagSize);

        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(_key, tagSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        return Encoding.UTF8.GetString(plaintext);
    }
}
