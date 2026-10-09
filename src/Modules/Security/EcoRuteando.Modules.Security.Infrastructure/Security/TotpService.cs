using System.Security.Cryptography;
using System.Text;
using EcoRuteando.Modules.Security.Application.Abstractions.Security;

namespace EcoRuteando.Modules.Security.Infrastructure.Security;

public sealed class TotpService : ITotpService
{
    private const int SecretLength = 20;
    private const int CodeLength = 6;
    private const int StepSeconds = 30;
    private const int ValidationWindow = 1;

    public string GenerateSecret()
    {
        var bytes = RandomNumberGenerator.GetBytes(SecretLength);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    public string GenerateQrCodeUri(string secret, string email, string issuer)
    {
        var encodedIssuer = Uri.EscapeDataString(issuer);
        var encodedEmail = Uri.EscapeDataString(email);
        var encodedSecret = Uri.EscapeDataString(secret);

        return $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={encodedSecret}&issuer={encodedIssuer}&digits={CodeLength}&period={StepSeconds}";
    }

    public bool ValidateCode(string secret, string code)
    {
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        for (var i = -ValidationWindow; i <= ValidationWindow; i++)
        {
            var timeStep = (currentTime / StepSeconds) + i;
            var expectedCode = ComputeTotp(secret, timeStep);

            if (CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expectedCode),
                Encoding.UTF8.GetBytes(code)))
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

    private static byte[] Base32Decode(string input)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        input = input.TrimEnd('=').ToUpperInvariant();

        var byteCount = input.Length * 5 / 8;
        var result = new byte[byteCount];

        var buffer = 0;
        var bitsInBuffer = 0;
        var index = 0;

        foreach (var c in input)
        {
            var val = alphabet.IndexOf(c);
            if (val < 0) continue;

            buffer = (buffer << 5) | val;
            bitsInBuffer += 5;

            if (bitsInBuffer >= 8)
            {
                bitsInBuffer -= 8;
                result[index++] = (byte)(buffer >> bitsInBuffer);
            }
        }

        return result;
    }
}
