using System.Security.Cryptography;
using System.Text;

namespace myProducts.Services
{
    public static class PasswordHelper
    {
        public static bool VerifyPassword(string password, byte[] storedPasswordHashWithSalt)
        {
            byte[] salt = new byte[16];
            Buffer.BlockCopy(storedPasswordHashWithSalt, 0, salt, 0, 16);

            byte[] storedHash = new byte[32];
            Buffer.BlockCopy(storedPasswordHashWithSalt, 16, storedHash, 0, 32);

            byte[] computedHash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                iterations: 100_000,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: 32
            );

            return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
        }

        public static byte[] HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                iterations: 100_000,
                HashAlgorithmName.SHA256,
                outputLength: 32
                );

            byte[] passwordHashWithSalt = new byte[salt.Length + hash.Length];
            Buffer.BlockCopy(salt, 0, passwordHashWithSalt, 0, salt.Length);
            Buffer.BlockCopy(hash, 0, passwordHashWithSalt, salt.Length, hash.Length);

            return passwordHashWithSalt;
        }
    }
}