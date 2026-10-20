using System.Text;

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

        public void Save(string content)
        {
            File.WriteAllText(_vaultPath, content, Encoding.UTF8);
        }

        public string Load()
        {
            if (!VaultExists())
            {
                throw new FileNotFoundException("Vault file was not found.");
            }

            return File.ReadAllText(_vaultPath, Encoding.UTF8);
        }
    }
}