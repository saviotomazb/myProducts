using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace myProducts.Services
{
    public static partial class PasswordService
    {
        [GeneratedRegex("[A-Z]")]
        private static partial Regex UpperRegex();

        [GeneratedRegex("[a-z]")]
        private static partial Regex LowerRegex();

        [GeneratedRegex("[0-9]")]
        private static partial Regex NumberRegex();

        [GeneratedRegex("[^A-Za-z0-9]")]
        private static partial Regex SpecialRegex();

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

        public static bool IsValid(string? password, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(password))
            {
                errorMessage = "Informe a senha";
                return false;
            }

            if (password.Length < 8)
            {
                errorMessage = "Mínimo de 8 caracteres";
                return false;
            }

            if (!UpperRegex().IsMatch(password))
            {
                errorMessage = "Pelo menos 1 letra maiúscula";
                return false;
            }

            if (!LowerRegex().IsMatch(password))
            {
                errorMessage = "Pelo menos 1 letra minúscula";
                return false;
            }

            if (!NumberRegex().IsMatch(password))
            {
                errorMessage = "Pelo menos 1 número";
                return false;
            }

            if (!SpecialRegex().IsMatch(password))
            {
                errorMessage = "Pelo menos 1 caractere especial";
                return false;
            }

            return true;
        }
    }
}