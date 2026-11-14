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
        private string? _currentOwnerId;

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

        public void CreateVault(
            string masterPassword)
        {
            LockVault();

            _salt =
                _authService.GenerateSalt();

            _key =
                _authService.DeriveKey(
                    masterPassword,
                    _salt
                );

            _currentOwnerId =
                CreateOwnerId(_key);

            _vaultData =
                new VaultData();

            SaveVault();
        }

        public bool UnlockVault(string masterPassword)
        {
            byte[]? temporaryKey = null;

            try
            {
                VaultFileData vaultFile = _storage.Load();

                _salt = Convert.FromBase64String(vaultFile.Salt);

                temporaryKey = _authService.DeriveKey(
                    masterPassword,
                    _salt
                );

                string decryptedJson =
                    _encryptionService.Decrypt(
                        vaultFile.EncryptedData,
                        temporaryKey
                    );

                VaultData? loadedVault =
                    JsonSerializer.Deserialize<VaultData>(
                        decryptedJson
                    );

                if (loadedVault == null)
                {
                    return false;
                }

                _key = temporaryKey;
                temporaryKey = null;
                
                _vaultData = loadedVault;

                _currentOwnerId = CreateOwnerId(_key);

                return true;
            }
            catch (CryptographicException)
            {
                return false;
            }
            catch (FormatException)
            {
                return false;
            }
            catch (JsonException)
            {
                return false;
            }
            finally
            {
                if (temporaryKey != null)
                {
                    CryptographicOperations.ZeroMemory(
                        temporaryKey
                    );
                }
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

        public void AddCredential(
            string service,
            string username,
            string password)
        {
            if (_vaultData == null ||
                _currentOwnerId == null)
            {
                throw new InvalidOperationException(
                    "Vault is not unlocked."
                );
            }

            var credential =
                new Credential
                {
                    OwnerId =
                        _currentOwnerId,

                    Service =
                        service,

                    Username =
                        username,

                    Password =
                        password
                };

            _vaultData.Credentials.Add(
                credential
            );

            SaveVault();
        }

        public List<Credential> GetCredentials()
        {
            if (_vaultData == null ||
                _currentOwnerId == null)
            {
                throw new InvalidOperationException(
                    "Vault is not unlocked."
                );
            }

            return _vaultData.Credentials
                .Where(
                    credential =>
                        credential.OwnerId ==
                        _currentOwnerId
                )
                .ToList();
        }

        public bool UpdateCredential(
            int index,
            string service,
            string username,
            string password)
        {
            if (_vaultData == null ||
                _currentOwnerId == null)
            {
                throw new InvalidOperationException(
                    "Vault is not unlocked."
                );
            }

            List<Credential> ownedCredentials =
                _vaultData.Credentials
                    .Where(
                        credential =>
                            credential.OwnerId ==
                            _currentOwnerId
                    )
                    .ToList();

            if (index < 0 ||
                index >= ownedCredentials.Count)
            {
                return false;
            }

            Credential credential =
                ownedCredentials[index];

            if (credential.OwnerId !=
                _currentOwnerId)
            {
                return false;
            }

            credential.Service = service;
            credential.Username = username;
            credential.Password = password;

            SaveVault();

            return true;
        }

        public bool DeleteCredential(
            int index)
        {
            if (_vaultData == null ||
                _currentOwnerId == null)
            {
                throw new InvalidOperationException(
                    "Vault is not unlocked."
                );
            }

            List<Credential> ownedCredentials =
                _vaultData.Credentials
                    .Where(
                        credential =>
                            credential.OwnerId ==
                            _currentOwnerId
                    )
                    .ToList();

            if (index < 0 ||
                index >= ownedCredentials.Count)
            {
                return false;
            }

            Credential credential =
                ownedCredentials[index];

            if (credential.OwnerId !=
                _currentOwnerId)
            {
                return false;
            }

            _vaultData.Credentials.Remove(
                credential
            );

            SaveVault();

            return true;
        }

        public void LockVault()
        {
            if (_key != null)
            {
                CryptographicOperations.ZeroMemory(
                    _key
                );

                _key = null;
            }

            if (_salt != null)
            {
                CryptographicOperations.ZeroMemory(
                    _salt
                );

                _salt = null;
            }

            _currentOwnerId = null;
            _vaultData = null;
        }

        private static string CreateOwnerId(
    byte[] key)
        {
            byte[] hash =
                SHA256.HashData(key);

            try
            {
                return Convert.ToHexString(hash);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(
                    hash
                );
            }
        }
    }
}
