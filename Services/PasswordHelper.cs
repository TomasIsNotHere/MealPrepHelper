using System.Security.Cryptography;
using System.Text;
using System;

namespace MealPrepHelper.Services
{
    public static class PasswordHelper
    {
        // hashing password using SHA256
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return string.Empty;

            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);

                var hash = sha256.ComputeHash(bytes);

                return Convert.ToHexString(hash);
            }
        }
        // verify password by comparing hashes
        public static bool VerifyPassword(string inputPassword, string storedHash)
        {
            var inputHash = HashPassword(inputPassword);
            return inputHash == storedHash;
        }
    }
}