using System;
using System.Security.Cryptography;
using System.Text;

namespace QADraft.Utilities
{
    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            var hashedInputPassword = HashPassword(password);
            return string.Equals(hashedInputPassword, hashedPassword, StringComparison.OrdinalIgnoreCase);
        }
    }
}
