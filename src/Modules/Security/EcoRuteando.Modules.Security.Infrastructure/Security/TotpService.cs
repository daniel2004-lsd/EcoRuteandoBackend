using System.Security.Cryptography;
using System.Text;
using EcoRuteando.Modules.Security.Application.Abstractions.Security;

namespace EcoRuteando.Modules.Security.Infrastructure.Security;

public sealed class TotpService : ITotpService
{
    private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
    private const int SecretLength = 20;
    private const int CodeLength = 6;
    private const int StepSeconds = 30;
    private const int ValidationWindow = 1;

    public string GenerateSecret()
    {
        var bytes = RandomNumberGenerator.GetBytes(SecretLength);
        return Base32Encode(bytes);
    }

    public string GenerateQrCodeUri(string secret, string email, string issuer)
    {
        var encodedIssuer = Uri.EscapeDataString(issuer);
        var encodedEmail = Uri.EscapeDataString(email);
        var cleanSecret = secret.Trim().ToUpperInvariant();

        return $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={cleanSecret}&issuer={encodedIssuer}&algorithm=SHA1&digits={CodeLength}&period={StepSeconds}";
    }

    public bool ValidateCode(string secret, string code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != CodeLength)
        {
            return false;
        }

        var cleanCode = code.Trim();
        var cleanSecret = secret.Trim().ToUpperInvariant();
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        for (var i = -ValidationWindow; i <= ValidationWindow; i++)
        {
            var timeStep = (currentTime / StepSeconds) + i;
            var expectedCode = ComputeTotp(cleanSecret, timeStep);

            if (CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expectedCode),
                Encoding.UTF8.GetBytes(cleanCode)))
            {
                return true;
            }
        }

        return false;
    }

    public string[] GenerateRecoveryCodes()
    {
        var codes = new string[8];
        for (var i = 0; i < 8; i++)
        {
            var bytes = RandomNumberGenerator.GetBytes(4);
            codes[i] = BitConverter.ToUInt32(bytes).ToString("D8");
        }
        return codes;
    }

    private static string ComputeTotp(string secret, long timeStep)
    {
        var secretBytes = Base32Decode(secret);
        var timeBytes = BitConverter.GetBytes(timeStep);

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(timeBytes);
        }

        using var hmac = new HMACSHA1(secretBytes);
        var hash = hmac.ComputeHash(timeBytes);

        var offset = hash[^1] & 0x0F;
        var code = ((hash[offset] & 0x7F) << 24)
                 | ((hash[offset + 1] & 0xFF) << 16)
                 | ((hash[offset + 2] & 0xFF) << 8)
                 | (hash[offset + 3] & 0xFF);

        var otp = code % (int)Math.Pow(10, CodeLength);
        return otp.ToString().PadLeft(CodeLength, '0');
    }

    private static string Base32Encode(byte[] data)
    {
        var result = new StringBuilder((data.Length * 8 + 4) / 5);
        int buffer = 0;
        int bitsInBuffer = 0;

        foreach (var b in data)
        {
            buffer = (buffer << 8) | b;
            bitsInBuffer += 8;

            while (bitsInBuffer >= 5)
            {
                bitsInBuffer -= 5;
                result.Append(Base32Alphabet[(buffer >> bitsInBuffer) & 0x1F]);
            }
        }

        if (bitsInBuffer > 0)
        {
            result.Append(Base32Alphabet[(buffer << (5 - bitsInBuffer)) & 0x1F]);
        }

        return result.ToString();
    }

    private static byte[] Base32Decode(string input)
    {
        input = input.TrimEnd('=').ToUpperInvariant();

        var byteCount = input.Length * 5 / 8;
        var result = new byte[byteCount];

        var buffer = 0;
        var bitsInBuffer = 0;
        var index = 0;

        foreach (var c in input)
        {
            var val = Base32Alphabet.IndexOf(c);
            if (val < 0) continue;

            buffer = (buffer << 5) | val;
            bitsInBuffer += 5;

            if (bitsInBuffer >= 8)
            {
                bitsInBuffer -= 8;
                if (index < result.Length)
                {
                    result[index++] = (byte)(buffer >> bitsInBuffer);
                }
            }
        }

        return result;
    }
}
