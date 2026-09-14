# Secure Password Manager Threat Model

## Overview

This document describes the main security threats that may affect the Secure Password Manager and the planned mitigations for them.

The threat model focuses on the areas required for Checkpoint 1:

- Master password
- Vault at rest
- Vault in memory
- User interface

## Threats and Mitigations

| Area | Threat | Planned Mitigation |
|---|---|---|
| Master password | The master password could be stored in plaintext | The master password will never be stored directly |
| Master password | An attacker could try to guess the master password | PBKDF2-HMAC-SHA256 with a random salt and a high iteration count will be used |
| Master password | The user could choose a weak master password | A minimum password length will be required |
| Vault at rest | An attacker could steal the vault file | The vault contents will be encrypted using AES-256-GCM |
| Vault at rest | An attacker could modify the encrypted vault | AES-GCM authentication will be used to detect tampering |
| Vault at rest | The encryption key could be stored together with the vault | The encryption key will be derived from the master password instead of being stored |
| Vault at rest | An AES-GCM nonce could be reused | A new cryptographically secure random nonce will be generated for each encryption |
| Vault in memory | Decrypted passwords could remain in memory longer than necessary | Sensitive values will only stay in memory while they are needed |
| Vault in memory | Sensitive information could be copied unnecessarily | The application will try to minimize temporary copies of passwords and keys |
| User interface | Passwords could be displayed on screen | Passwords will not be shown in cleartext during normal listing operations |
| User interface | The master password could be visible while typing | The application will read the master password without displaying it on screen |
| User interface | Malicious or invalid input could cause unexpected behaviour | All user input will be validated before it is processed |
| Logging | Passwords or encryption keys could be written to logs | Sensitive information will never be logged |
| Error handling | Internal exception details could be shown to the user | Generic user-facing error messages will be used |
| Access control | One user could access another user's stored credentials | Ownership checks will be enforced for every credential operation |

## Main Assets

The main assets that need to be protected are:

- Master password
- Encryption key
- Stored usernames
- Stored passwords
- Vault contents
- User identity information

## Attacker Assumptions

The application assumes that an attacker may:

- Obtain a copy of the encrypted vault file
- Modify the vault file
- Attempt to guess the master password
- Enter malicious or unexpected input
- Access application log files

The goal of the design is to prevent these actions from exposing the stored credentials.