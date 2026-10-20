# Secure Password Manager

## Project Overview

Secure Password Manager is a C# console application developed for the
ICS0022 Secure Programming course.

The purpose of the project is to securely store and manage user credentials.
The application will focus on secure authentication, encryption, input
validation, access control, secure memory handling, logging, and error handling.

## Planned Features

The application will allow the user to:

- Create a password vault
- Unlock the vault using a master password
- Add credentials
- List stored credential entries
- Retrieve credentials
- Update credentials
- Delete credentials
- Lock the vault

Stored passwords will not be displayed in cleartext during normal listing
operations.

## Technology

- Language: C#
- Framework: .NET
- Interface: Console application
- IDE: Visual Studio 2022

## Planned Security Design

The initial security design includes:

- AES-256-GCM for vault encryption
- PBKDF2-HMAC-SHA256 for deriving an encryption key from the master password
- Random salts and nonces generated using cryptographically secure randomness
- Input validation for user input
- Secure error handling
- Logging without passwords, keys, or decrypted credentials

## Planned Components

- Authentication Module
- Encryption Module
- Vault Service
- Storage Module
- Validation Module
- Logging Module

## Build and Run

Open the solution in Visual Studio 2022.

Build the project using:

Build -> Build Solution

Run the application using:

Debug -> Start Without Debugging

The application can also be run from the command line:

dotnet run --project SecurePasswordManager

## Checkpoint 2 Status

The following core features are implemented:

- Basic vault storage
- Master password authentication
- PBKDF2-HMAC-SHA256 key derivation
- AES-256-GCM encryption
- Encrypted credential storage
- Add credential
- List credentials
- Update credential
- Delete credential
- Hidden password input
- Basic input validation
- Passwords are not displayed in cleartext

## Build Instructions

Requirements:

- Windows
- .NET SDK
- Visual Studio 2022 or another environment capable of building .NET projects

### Using Visual Studio 2022

1. Clone or download the repository.
2. Open `SecurePasswordManager.sln`.
3. Build the solution using:

   `Build -> Build Solution`

4. Run the application using:

   `Debug -> Start Without Debugging`

### Using the Command Line

From the repository directory:

```bash
dotnet build