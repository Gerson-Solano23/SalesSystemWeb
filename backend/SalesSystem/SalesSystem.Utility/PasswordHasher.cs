using System;
using System.Security.Cryptography;
using System.Text;

namespace SalesSystem.Utility
{
    /// <summary>
    /// PBKDF2 (SHA-256) password hashing. Stored format: PBKDF2$iterations$salt$hash (base64).
    /// </summary>
    public static class PasswordHasher
    {
        private const string Prefix = "PBKDF2";
        private const int Iterations = 100_000;
        private const int SaltSize = 16;
        private const int KeySize = 32;

        public static string Hash(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password cannot be empty", nameof(password));
            }

            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

            return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
        }

        public static bool IsHashed(string? storedPassword)
        {
            return storedPassword != null && storedPassword.StartsWith(Prefix + "$", StringComparison.Ordinal);
        }

        /// <summary>
        /// Verifies a password against the stored value. Values stored before hashing was introduced
        /// (plain text) are still accepted, and <paramref name="needsRehash"/> tells the caller to upgrade them.
        /// </summary>
        public static bool Verify(string? password, string? storedPassword, out bool needsRehash)
        {
            needsRehash = false;
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedPassword))
            {
                return false;
            }

            if (!IsHashed(storedPassword))
            {
                bool legacyMatch = CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(password),
                    Encoding.UTF8.GetBytes(storedPassword));
                needsRehash = legacyMatch;
                return legacyMatch;
            }

            string[] parts = storedPassword.Split('$');
            if (parts.Length != 4 || !int.TryParse(parts[1], out int iterations))
            {
                return false;
            }

            byte[] salt;
            byte[] expected;
            try
            {
                salt = Convert.FromBase64String(parts[2]);
                expected = Convert.FromBase64String(parts[3]);
            }
            catch (FormatException)
            {
                return false;
            }

            byte[] actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
            bool match = CryptographicOperations.FixedTimeEquals(actual, expected);
            needsRehash = match && iterations < Iterations;
            return match;
        }
    }
}
