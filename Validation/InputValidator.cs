namespace SecurePasswordManager.Validation
{
    public static class InputValidator
    {
        private const int MaxServiceLength = 100;
        private const int MaxUsernameLength = 50;
        private const int MaxPasswordLength = 200;

        public static bool IsValidService(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            value = value.Trim();

            if (value.Length > MaxServiceLength)
            {
                return false;
            }

            if (ContainsControlCharacters(value))
            {
                return false;
            }

            return true;
        }

        public static bool IsValidUsername(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            value = value.Trim();

            if (value.Length > MaxUsernameLength)
            {
                return false;
            }

            if (ContainsControlCharacters(value))
            {
                return false;
            }

            return true;
        }

        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            if (password.Length < 8)
            {
                return false;
            }

            if (password.Length > MaxPasswordLength)
            {
                return false;
            }

            if (ContainsControlCharacters(password))
            {
                return false;
            }

            return true;
        }

        public static bool IsValidMenuChoice(string value, int min, int max)
        {
            if (!int.TryParse(value, out int choice))
            {
                return false;
            }

            return choice >= min && choice <= max;
        }

        public static bool IsValidCredentialNumber(
            string value,
            int credentialCount,
            out int index)
        {
            index = -1;

            if (!int.TryParse(value, out int number))
            {
                return false;
            }

            if (number < 1 || number > credentialCount)
            {
                return false;
            }

            index = number - 1;

            return true;
        }

        public static string SanitizeText(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Trim();
        }

        private static bool ContainsControlCharacters(string value)
        {
            foreach (char character in value)
            {
                if (char.IsControl(character))
                {
                    return true;
                }
            }

            return false;
        }
    }
}