namespace Syncify.Connections.Application.Ports;

public interface ITokenEncryptor
{
    string Encrypt(string plaintext);
    string Decrypt(string ciphertext);
}