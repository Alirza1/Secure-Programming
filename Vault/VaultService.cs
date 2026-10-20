using System.Security.Cryptography;
using System.Text.Json;
using SecurePasswordManager.Authentication;
using SecurePasswordManager.Encryption;
using SecurePasswordManager.Storage;

namespace SecurePasswordManager.Vault
{
    public class VaultService
    {
        private readonly VaultStorage _storage;
        private readonly MasterPasswordService _authService;
        private readonly EncryptionService _encryptionService;

        private VaultData? _vaultData;
        private byte[]? _key;
        private byte[]? _salt;

        public VaultService(
            VaultStorage storage,
            MasterPasswordService authService,
            EncryptionService encryptionService)
        {
            _storage = storage;
            _authService = authService;
            _encryptionService = encryptionService;
        }

        public bool VaultExists()
        {
            return _storage.VaultExists();
        }

        public void CreateVault(string masterPassword)
        {
            _salt = _authService.GenerateSalt();
            _key = _authService.DeriveKey(masterPassword, _salt);

            _vaultData = new VaultData();

            SaveVault();
        }

        public bool UnlockVault(string masterPassword)
        {
            try
            {
                VaultFileData vaultFile = _storage.Load();

                _salt = Convert.FromBase64String(vaultFile.Salt);

                byte[] key = _authService.DeriveKey(
                    masterPassword,
                    _salt
                );

                string decryptedJson =
                    _encryptionService.Decrypt(
                        vaultFile.EncryptedData,
                        key
                    );

                VaultData? loadedVault =
                    JsonSerializer.Deserialize<VaultData>(
                        decryptedJson
                    );

                if (loadedVault == null)
                {
                    return false;
                }

                _key = key;
                _vaultData = loadedVault;

                return true;
            }
            catch (
                CryptographicException)
            {
                return false;
            }
            catch (
                FormatException)
            {
                return false;
            }
            catch (
                JsonException)
            {
                return false;
            }
        }

        public void SaveVault()
        {
            if (_vaultData == null ||
                _key == null ||
                _salt == null)
            {
                throw new InvalidOperationException(
                    "Vault is not unlocked."
                );
            }

            string json =
                JsonSerializer.Serialize(_vaultData);

            string encryptedData =
                _encryptionService.Encrypt(
                    json,
                    _key
                );

            var vaultFile = new VaultFileData
            {
                Salt = Convert.ToBase64String(_salt),
                EncryptedData = encryptedData
            };

            _storage.Save(vaultFile);
        }

        public void AddCredential(string service, string username, string password)
        {
            if (_vaultData == null)
            {
                throw new InvalidOperationException("Vault is not unlocked.");
            }

            var credential = new Credential
            {
                Service = service,
                Username = username,
                Password = password
            };

            _vaultData.Credentials.Add(credential);

            SaveVault();
        }

        public List<Credential> GetCredentials()
        {
            if (_vaultData == null)
            {
                throw new InvalidOperationException("Vault is not unlocked.");
            }

            return _vaultData.Credentials;
        }

        public bool UpdateCredential(
            int index,
            string service,
            string username,
            string password)
        {
            if (_vaultData == null)
            {
                throw new InvalidOperationException("Vault is not unlocked.");
            }

            if (index < 0 || index >= _vaultData.Credentials.Count)
            {
                return false;
            }

            Credential credential = _vaultData.Credentials[index];

            credential.Service = service;
            credential.Username = username;
            credential.Password = password;

            SaveVault();

            return true;
        }

        public bool DeleteCredential(int index)
        {
            if (_vaultData == null)
            {
                throw new InvalidOperationException("Vault is not unlocked.");
            }

            if (index < 0 || index >= _vaultData.Credentials.Count)
            {
                return false;
            }

            _vaultData.Credentials.RemoveAt(index);

            SaveVault();

            return true;
        }
    }
}
