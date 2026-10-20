namespace SecurePasswordManager.Storage
{
    public class VaultFileData
    {
        public string Salt { get; set; } = string.Empty;

        public string EncryptedData { get; set; } = string.Empty;
    }
}