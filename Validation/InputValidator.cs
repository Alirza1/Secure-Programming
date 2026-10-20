namespace SecurePasswordManager.Validation
{
    public static class InputValidator
    {
        public static bool IsValidRequiredText(string value, int maxLength = 200)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            if (value.Length > maxLength)
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

            if (password.Length > 200)
            {
                return false;
            }

            return true;
        }
    }
}