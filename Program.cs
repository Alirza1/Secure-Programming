using SecurePasswordManager.Authentication;
using SecurePasswordManager.Encryption;
using SecurePasswordManager.Logging;
using SecurePasswordManager.Storage;
using SecurePasswordManager.Validation;
using SecurePasswordManager.Vault;

var storage = new VaultStorage("vault.dat");
var authService = new MasterPasswordService();
var encryptionService = new EncryptionService();
var securityLogger = new SecurityLogger("security.log");

var vaultService = new VaultService(
    storage,
    authService,
    encryptionService
);

// Handle Ctrl+C safely
Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;

    vaultService.LockVault();

    securityLogger.Log(
        "ApplicationExit",
        "CancelledByUser"
    );

    Console.WriteLine();
    Console.WriteLine(
        "Vault locked. Exiting safely."
    );

    Environment.Exit(0);
};

try
{
    RunApplication(
        vaultService,
        securityLogger
    );
}
catch (Exception)
{
    securityLogger.Log(
        "ApplicationError",
        "Failure"
    );

    Console.WriteLine(
        "An unexpected error occurred. Please try again."
    );
}
finally
{
    vaultService.LockVault();
}


static void RunApplication(
    VaultService vaultService,
    SecurityLogger securityLogger)
{
    Console.WriteLine("Secure Password Manager");
    Console.WriteLine();

    // CREATE NEW VAULT
    if (!vaultService.VaultExists())
    {
        Console.WriteLine("No vault found.");
        Console.Write("Create master password: ");

        string masterPassword = ReadPassword();

        if (!InputValidator.IsValidPassword(masterPassword))
        {
            Console.WriteLine(
                "Master password must be between 8 and 200 characters and contain no control characters."
            );

            return;
        }

        try
        {
            vaultService.CreateVault(
                masterPassword
            );

            securityLogger.Log(
                "VaultCreation",
                "Success"
            );

            Console.WriteLine(
                "Vault created successfully."
            );
        }
        catch (Exception)
        {
            securityLogger.Log(
                "VaultCreation",
                "Failure"
            );

            Console.WriteLine(
                "Unable to create the vault."
            );

            return;
        }
    }

    // UNLOCK EXISTING VAULT
    else
    {
        Console.Write(
            "Enter master password: "
        );

        string masterPassword =
            ReadPassword();

        bool unlocked =
            vaultService.UnlockVault(
                masterPassword
            );

        if (!unlocked)
        {
            securityLogger.Log(
                "VaultUnlock",
                "Failure"
            );

            Console.WriteLine(
                "Authentication failed."
            );

            return;
        }

        securityLogger.Log(
            "VaultUnlock",
            "Success"
        );

        Console.WriteLine(
            "Vault unlocked successfully."
        );
    }

    bool running = true;

    while (running)
    {
        Console.WriteLine();
        Console.WriteLine(
            "1. Add credential"
        );
        Console.WriteLine(
            "2. List credentials"
        );
        Console.WriteLine(
            "3. Update credential"
        );
        Console.WriteLine(
            "4. Delete credential"
        );
        Console.WriteLine(
            "5. Exit"
        );
        Console.WriteLine();

        Console.Write(
            "Choose an option: "
        );

        string choice =
            Console.ReadLine()
            ?? string.Empty;

        // Validate menu input
        if (!InputValidator
                .IsValidMenuChoice(
                    choice,
                    1,
                    5))
        {
            Console.WriteLine(
                "Invalid option."
            );

            continue;
        }

        switch (choice)
        {
            case "1":
                AddCredential(
                    vaultService,
                    securityLogger
                );
                break;

            case "2":
                ListCredentials(
                    vaultService
                );
                break;

            case "3":
                UpdateCredential(
                    vaultService,
                    securityLogger
                );
                break;

            case "4":
                DeleteCredential(
                    vaultService,
                    securityLogger
                );
                break;

            case "5":
                vaultService.LockVault();

                securityLogger.Log(
                    "VaultLock",
                    "Success"
                );

                running = false;

                Console.WriteLine(
                    "Vault locked. Goodbye."
                );

                break;
        }
    }
}


static void AddCredential(
    VaultService vaultService,
    SecurityLogger securityLogger)
{
    Console.Write(
        "Service: "
    );

    string service =
        InputValidator.SanitizeText(
            Console.ReadLine()
            ?? string.Empty
        );

    if (!InputValidator
            .IsValidService(service))
    {
        Console.WriteLine(
            "Invalid service name."
        );

        return;
    }

    Console.Write(
        "Username: "
    );

    string username =
        InputValidator.SanitizeText(
            Console.ReadLine()
            ?? string.Empty
        );

    if (!InputValidator
            .IsValidUsername(username))
    {
        Console.WriteLine(
            "Invalid username."
        );

        return;
    }

    Console.Write(
        "Password: "
    );

    string password =
        ReadPassword();

    if (!InputValidator
            .IsValidPassword(password))
    {
        Console.WriteLine(
            "Password must be between 8 and 200 characters and contain no control characters."
        );

        return;
    }

    try
    {
        vaultService.AddCredential(
            service,
            username,
            password
        );

        securityLogger.Log(
            "CredentialAdd",
            "Success"
        );

        Console.WriteLine(
            "Credential added successfully."
        );
    }
    catch (Exception)
    {
        securityLogger.Log(
            "CredentialAdd",
            "Failure"
        );

        Console.WriteLine(
            "Unable to add credential."
        );
    }
}


static void ListCredentials(
    VaultService vaultService)
{
    try
    {
        List<Credential> credentials =
            vaultService.GetCredentials();

        if (credentials.Count == 0)
        {
            Console.WriteLine(
                "No credentials stored."
            );

            return;
        }

        Console.WriteLine();
        Console.WriteLine(
            "Stored credentials:"
        );

        for (
            int i = 0;
            i < credentials.Count;
            i++)
        {
            Console.WriteLine(
                $"{i + 1}. {credentials[i].Service}"
            );

            Console.WriteLine(
                $"   Username: {credentials[i].Username}"
            );

            // Never print password
            Console.WriteLine(
                "   Password: ********"
            );
        }
    }
    catch (Exception)
    {
        Console.WriteLine(
            "Unable to list credentials."
        );
    }
}


static void UpdateCredential(
    VaultService vaultService,
    SecurityLogger securityLogger)
{
    List<Credential> credentials;

    try
    {
        credentials =
            vaultService.GetCredentials();
    }
    catch (Exception)
    {
        securityLogger.Log(
            "CredentialUpdate",
            "Failure"
        );

        Console.WriteLine(
            "Unable to access credentials."
        );

        return;
    }

    if (credentials.Count == 0)
    {
        Console.WriteLine(
            "No credentials stored."
        );

        return;
    }

    ListCredentials(
        vaultService
    );

    Console.Write(
        "Credential number to update: "
    );

    string input =
        Console.ReadLine()
        ?? string.Empty;

    if (!InputValidator
            .IsValidCredentialNumber(
                input,
                credentials.Count,
                out int index))
    {
        Console.WriteLine(
            "Invalid credential number."
        );

        return;
    }

    Console.Write(
        "New service: "
    );

    string service =
        InputValidator.SanitizeText(
            Console.ReadLine()
            ?? string.Empty
        );

    if (!InputValidator
            .IsValidService(service))
    {
        Console.WriteLine(
            "Invalid service name."
        );

        return;
    }

    Console.Write(
        "New username: "
    );

    string username =
        InputValidator.SanitizeText(
            Console.ReadLine()
            ?? string.Empty
        );

    if (!InputValidator
            .IsValidUsername(username))
    {
        Console.WriteLine(
            "Invalid username."
        );

        return;
    }

    Console.Write(
        "New password: "
    );

    string password =
        ReadPassword();

    if (!InputValidator
            .IsValidPassword(password))
    {
        Console.WriteLine(
            "Password must be between 8 and 200 characters and contain no control characters."
        );

        return;
    }

    try
    {
        bool updated =
            vaultService.UpdateCredential(
                index,
                service,
                username,
                password
            );

        if (updated)
        {
            securityLogger.Log(
                "CredentialUpdate",
                "Success"
            );

            Console.WriteLine(
                "Credential updated successfully."
            );
        }
        else
        {
            securityLogger.Log(
                "CredentialUpdate",
                "Failure"
            );

            Console.WriteLine(
                "Credential not found."
            );
        }
    }
    catch (Exception)
    {
        securityLogger.Log(
            "CredentialUpdate",
            "Failure"
        );

        Console.WriteLine(
            "Unable to update credential."
        );
    }
}


static void DeleteCredential(
    VaultService vaultService,
    SecurityLogger securityLogger)
{
    List<Credential> credentials;

    try
    {
        credentials =
            vaultService.GetCredentials();
    }
    catch (Exception)
    {
        securityLogger.Log(
            "CredentialDelete",
            "Failure"
        );

        Console.WriteLine(
            "Unable to access credentials."
        );

        return;
    }

    if (credentials.Count == 0)
    {
        Console.WriteLine(
            "No credentials stored."
        );

        return;
    }

    ListCredentials(
        vaultService
    );

    Console.Write(
        "Credential number to delete: "
    );

    string input =
        Console.ReadLine()
        ?? string.Empty;

    if (!InputValidator
            .IsValidCredentialNumber(
                input,
                credentials.Count,
                out int index))
    {
        Console.WriteLine(
            "Invalid credential number."
        );

        return;
    }

    try
    {
        bool deleted =
            vaultService.DeleteCredential(
                index
            );

        if (deleted)
        {
            securityLogger.Log(
                "CredentialDelete",
                "Success"
            );

            Console.WriteLine(
                "Credential deleted successfully."
            );
        }
        else
        {
            securityLogger.Log(
                "CredentialDelete",
                "Failure"
            );

            Console.WriteLine(
                "Credential not found."
            );
        }
    }
    catch (Exception)
    {
        securityLogger.Log(
            "CredentialDelete",
            "Failure"
        );

        Console.WriteLine(
            "Unable to delete credential."
        );
    }
}


static string ReadPassword()
{
    string password =
        string.Empty;

    while (true)
    {
        ConsoleKeyInfo key =
            Console.ReadKey(
                intercept: true
            );

        if (key.Key ==
            ConsoleKey.Enter)
        {
            Console.WriteLine();
            break;
        }

        if (key.Key ==
            ConsoleKey.Backspace)
        {
            if (password.Length > 0)
            {
                password =
                    password[..^1];
            }

            continue;
        }

        if (!char.IsControl(
                key.KeyChar))
        {
            password +=
                key.KeyChar;
        }
    }

    return password;
}