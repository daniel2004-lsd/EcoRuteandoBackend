namespace EcoRuteando.Modules.Security.Application.Abstractions.Security;

public interface IEncryptionService
{
    byte[] Encrypt(byte[] plainBytes);
    byte[] Decrypt(byte[] cipherBytes);
}
