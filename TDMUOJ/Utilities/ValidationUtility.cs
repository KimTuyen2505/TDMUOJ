using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace TDMUOJ.Utilities
{
    public static class ValidationUtility
    {
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Sử dụng regex để kiểm tra định dạng email
                var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                return Regex.IsMatch(email, emailPattern);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            // Kiểm tra độ dài tối thiểu của username
            if (username.Length < 6)
                return false;

            // Kiểm tra username không chứa ký tự đặc biệt
            return Regex.IsMatch(username, @"^[a-zA-Z0-9]+$");
        }

        public static bool IsStrongPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            // Kiểm tra độ dài tối thiểu
            if (password.Length < 8)
                return false;

            // Kiểm tra các yêu cầu về ký tự
            bool hasUppercase = password.Any(char.IsUpper);
            bool hasLowercase = password.Any(char.IsLower);
            bool hasNumber = password.Any(char.IsDigit);
            bool hasSpecialCharacter = password.Any(c => !char.IsLetterOrDigit(c));

            return hasUppercase && hasLowercase && hasNumber && hasSpecialCharacter;
        }

        public static bool IsSafeString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return true;

            // Kiểm tra các ký tự nguy hiểm có thể dùng cho XSS hoặc SQL Injection
            var dangerousPatterns = new[]
            {
                @"<script>", @"</script>", @"javascript:", @"vbscript:",
                @"onload=", @"onerror=", @"onclick=", @"onmouseover=",
                @"SELECT.*FROM", @"INSERT.*INTO", @"DELETE.*FROM", @"DROP.*TABLE",
                @"UNION.*SELECT", @"EXEC.*sp_", @"EXEC.*xp_"
            };

            return !dangerousPatterns.Any(pattern => Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase));
        }

        public static string SanitizeInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            // Loại bỏ các ký tự HTML và JavaScript nguy hiểm
            input = HttpUtility.HtmlEncode(input);

            // Loại bỏ các ký tự đặc biệt nguy hiểm
            input = Regex.Replace(input, @"[;'\\]", "");

            return input;
        }
    }
}
