namespace SecurePasswordManager.Vault
{
    public class Credential
    {
        public string Id { get; set; } =
            Guid.NewGuid().ToString();

        public string OwnerId { get; set; } =
            string.Empty;

        public string Service { get; set; } =
            string.Empty;

        public string Username { get; set; } =
            string.Empty;

        public string Password { get; set; } =
            string.Empty;
    }
}