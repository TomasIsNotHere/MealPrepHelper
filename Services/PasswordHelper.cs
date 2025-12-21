using System.Security.Cryptography;
using System.Text;
using System;

namespace MealPrepHelper.Services
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return string.Empty;

            using (var sha256 = SHA256.Create())
            {
                // Převedeme text na bajty
                var bytes = Encoding.UTF8.GetBytes(password);
                
                // Vypočítáme hash
                var hash = sha256.ComputeHash(bytes);
                
                // Převedeme hash zpět na čitelný řetězec (hex)
                return Convert.ToHexString(hash);
            }
        }

        public static bool VerifyPassword(string inputPassword, string storedHash)
        {
            var inputHash = HashPassword(inputPassword);
            return inputHash == storedHash;
        }
    }
}