# Secure Password Manager Architecture

## Overview

The Secure Password Manager is a C# console application developed for the ICS0022 Secure Programming course.

The application is designed to securely store and manage user credentials. The architecture separates authentication, encryption, credential management, storage, validation, and logging into different components.

## Main Components

### Console Interface

The Console Interface is responsible for communication between the user and the application.

Its responsibilities include:

- displaying menus;
- reading user input;
- sending requests to the correct application component;
- showing safe responses and error messages.

The interface should not display stored passwords in cleartext during normal use.

### Authentication Module

The Authentication Module is responsible for verifying the master password.

The master password itself will not be stored in plaintext.

A key will be derived from the master password using a password-based key derivation function.

### Vault Service

The Vault Service manages credential records stored in the password manager.

It will support operations such as:

- adding credentials;
- retrieving credentials;
- updating credentials;
- deleting credentials;
- listing stored credential entries.

### Encryption Module

The Encryption Module is responsible for protecting sensitive vault data.

The planned encryption algorithm is AES-256-GCM.

AES-GCM provides:

- confidentiality;
- integrity;
- authentication of encrypted data.

The encryption key will be derived from the user's master password.

### Storage Module

The Storage Module is responsible for reading and writing the vault file.

Sensitive credential information must be encrypted before being written to disk.

### Validation Module

The Validation Module checks user input before it is processed.

Its purpose is to reduce the risk of invalid or malicious input causing unexpected behaviour.

### Logging Module

The Logging Module records important application events such as:

- successful login;
- failed login;
- credential creation;
- credential update;
- credential deletion;
- vault locking.

Passwords, encryption keys, and decrypted credentials must never be written to the logs.

## Data Flow

The planned data flow is:

1. The user starts the application.
2. The Console Interface asks for the master password.
3. The Authentication Module processes the master password.
4. An encryption key is derived from the master password.
5. The Storage Module loads the encrypted vault file.
6. The Encryption Module decrypts the vault.
7. The Vault Service performs the requested credential operation.
8. If the vault is modified, it is encrypted again.
9. The Storage Module writes the encrypted vault back to disk.

## Architecture Diagram

```text

                 +-------------------+
                 |       User        |
                 +---------+---------+
                           |
                           v
                 +-------------------+
                 | Console Interface |
                 +---------+---------+
                           |
              +------------+------------+
              |                         |
              v                         v
    +-------------------+     +-------------------+
    | Authentication    |     |   Vault Service   |
    | Module            |     +---------+---------+
    +---------+---------+               |
              |                 +-------+-------+
              |                 |               |
              v                 v               v
    +-------------------+ +-------------+ +-------------+
    | Key Derivation    | | Encryption  | | Storage     |
    | Module            | | Module      | | Module      |
    +-------------------+ +------+------+ +------+------+
                                |               |
                                v               v
                         Encrypted Vault     Vault File