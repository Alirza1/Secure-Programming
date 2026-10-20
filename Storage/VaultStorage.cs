using System.Text.Json;

namespace SecurePasswordManager.Storage
{
    public class VaultStorage
    {
        private readonly string _vaultPath;

        public VaultStorage(string vaultPath)
        {
            _vaultPath = vaultPath;
        }

        public bool VaultExists()
        {
            return File.Exists(_vaultPath);
        }

        public void Save(VaultFileData vaultFileData)
        {
            string json = JsonSerializer.Serialize(vaultFileData);

            File.WriteAllText(_vaultPath, json);
        }

        public VaultFileData Load()
        {
            if (!VaultExists())
            {
                throw new FileNotFoundException("Vault file was not found.");
            }

            string json = File.ReadAllText(_vaultPath);

            VaultFileData? vaultFileData =
                JsonSerializer.Deserialize<VaultFileData>(json);

            if (vaultFileData == null)
            {
                throw new InvalidDataException("Invalid vault file.");
            }

            return vaultFileData;
        }
    }
}