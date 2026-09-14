# Vault Format and Cryptographic Design

## Overview

The Secure Password Manager will store user credentials inside an encrypted vault file.

The vault will be encrypted before it is written to disk, so usernames, passwords, and other sensitive data are not stored in plaintext.

The master password will not be stored directly. Instead, it will be used to derive the encryption key required to unlock the vault.

The planned cryptographic design uses:

- PBKDF2-HMAC-SHA256 for key derivation
- AES-256-GCM for vault encryption
- A random salt for key derivation
- A random nonce for each encryption operation

## Vault Format

The vault will use JSON as the internal data format because it is simple to work with in C# and easy to serialize and deserialize.

Before the vault is saved to disk, the JSON content will be encrypted.

Example of the plaintext vault structure before encryption:

```json
{
  "credentials": [
    {
      "service": "GitHub",
      "username": "user@example.com",
      "password": "example-password"
    }
  ]
}

## Key Derivation

The master password will not be used directly as the AES encryption key.

Instead, the application will derive a 256-bit encryption key from the master password using PBKDF2-HMAC-SHA256.

The key derivation process is:

1. The user enters the master password.
2. The application reads the random salt stored in the vault file.
3. PBKDF2 processes the master password together with the salt.
4. PBKDF2 performs many iterations to make password guessing slower.
5. The result is a 256-bit key used by AES-256-GCM.

The master password itself will not be stored in the vault file.

The derived encryption key will also not be stored permanently on disk.

The vault file will contain the salt because the same salt is required when deriving the key again during the next login.

### Key Derivation Flow

```text
Master Password
      |
      v
   PBKDF2
      +
 Random Salt
      |
      v
256-bit Encryption Key
      |
      v
 AES-256-GCM

## Vault Encryption and Decryption

The vault will be encrypted using AES-256-GCM.

AES-GCM was selected because it provides both confidentiality and integrity. This means that the vault contents are hidden from an attacker, and unauthorized modification of the encrypted data can also be detected.

### Encryption Process

When the vault is saved:

1. The application serializes the credential data into JSON.
2. A new random nonce is generated.
3. The derived 256-bit key is used with AES-256-GCM.
4. The plaintext JSON is encrypted.
5. AES-GCM produces:
   - ciphertext;
   - authentication tag.
6. The salt, nonce, ciphertext, and authentication tag are stored in the vault file.

The plaintext vault data must not be written directly to disk.

### Decryption Process

When the user unlocks the vault:

1. The application reads the salt, nonce, ciphertext, and authentication tag from the vault file.
2. The user enters the master password.
3. PBKDF2 derives the same 256-bit encryption key using the stored salt.
4. AES-256-GCM attempts to decrypt the ciphertext.
5. The authentication tag is checked.
6. If authentication succeeds, the plaintext vault data is loaded into memory.
7. If authentication fails, the vault must not be opened and a generic error should be shown to the user.

### Encryption Flow

```text
Credential Data
      |
      v
 Serialize to JSON
      |
      v
 AES-256-GCM
   + Key
   + Nonce
      |
      v
 Ciphertext + Authentication Tag
      |
      v
   Vault File


Vault File
   |
   v
Salt + Nonce + Ciphertext + Tag
   |
   v
PBKDF2 + Master Password
   |
   v
256-bit Key
   |
   v
AES-256-GCM
   |
   v
Plaintext Vault Data

## Stored and Non-Stored Sensitive Data

The vault file may store the following values:

- Salt
- AES-GCM nonce
- Ciphertext
- Authentication tag

These values are required to derive the encryption key and decrypt the vault.

The following sensitive values must never be stored in plaintext on disk:

- Master password
- Derived encryption key
- Decrypted passwords
- Decrypted vault contents

The master password is entered by the user when the vault is unlocked. The encryption key is derived when needed and should only remain in memory for as long as necessary.

Application logs must also never contain passwords, encryption keys, or decrypted credential data.