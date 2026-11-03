using System;
using System.Security.Cryptography;

namespace BookStore.Utilities
{
    public static class PasswordUtil
    {
        public static bool Verify(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash) || !storedHash.Contains(":")) return false;

            var parts = storedHash.Split(':');
            if (parts.Length != 3) return false;

            int iterations = int.Parse(parts[0]);
            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] expectedHash = Convert.FromBase64String(parts[2]);

            byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA1, expectedHash.Length);
            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
    }
}