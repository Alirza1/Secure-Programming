using SecurePasswordManager.Authentication;
using SecurePasswordManager.Encryption;
using SecurePasswordManager.Storage;
using SecurePasswordManager.Vault;
using SecurePasswordManager.Validation;

var storage = new VaultStorage("vault.dat");
var authService = new MasterPasswordService();
var encryptionService = new EncryptionService();

var vaultService = new VaultService(
    storage,
    authService,
    encryptionService
);

Console.WriteLine("Secure Password Manager");
Console.WriteLine();

if (!vaultService.VaultExists())
{
    Console.WriteLine("No vault found.");
    Console.Write("Create master password: ");

    string masterPassword = ReadPassword();

    if (!InputValidator.IsValidPassword(masterPassword))
    {
        Console.WriteLine("Master password must be at least 8 characters long.");
        return;
    }

    vaultService.CreateVault(masterPassword);

    Console.WriteLine("Vault created successfully.");
}
else
{
    Console.Write("Enter master password: ");

    string masterPassword = ReadPassword();

    bool unlocked = vaultService.UnlockVault(masterPassword);

    if (!unlocked)
    {
        Console.WriteLine("Authentication failed.");
        return;
    }

    Console.WriteLine("Vault unlocked successfully.");
}

bool running = true;

while (running)
{
    Console.WriteLine();
    Console.WriteLine("1. Add credential");
    Console.WriteLine("2. List credentials");
    Console.WriteLine("3. Update credential");
    Console.WriteLine("4. Delete credential");
    Console.WriteLine("5. Exit");
    Console.WriteLine();

    Console.Write("Choose an option: ");
    string choice = Console.ReadLine() ?? string.Empty;

    switch (choice)
    {
        case "1":
            AddCredential(vaultService);
            break;

        case "2":
            ListCredentials(vaultService);
            break;

        case "3":
            UpdateCredential(vaultService);
            break;

        case "4":
            DeleteCredential(vaultService);
            break;

        case "5":
            running = false;
            Console.WriteLine("Vault locked. Goodbye.");
            break;

        default:
            Console.WriteLine("Invalid option.");
            break;
    }
}

static void AddCredential(VaultService vaultService)
{
    Console.Write("Service: ");
    string service = Console.ReadLine() ?? string.Empty;

    if (!InputValidator.IsValidRequiredText(service))
    {
        Console.WriteLine("Invalid service name.");
        return;
    }

    Console.Write("Username: ");
    string username = Console.ReadLine() ?? string.Empty;

    if (!InputValidator.IsValidRequiredText(username))
    {
        Console.WriteLine("Invalid username.");
        return;
    }

    Console.Write("Password: ");
    string password = ReadPassword();

    if (!InputValidator.IsValidPassword(password))
    {
        Console.WriteLine("Password must be at least 8 characters long.");
        return;
    }

    vaultService.AddCredential(
        service,
        username,
        password
    );

    Console.WriteLine("Credential added successfully.");
}

static void ListCredentials(VaultService vaultService)
{
    List<Credential> credentials =
        vaultService.GetCredentials();

    if (credentials.Count == 0)
    {
        Console.WriteLine("No credentials stored.");
        return;
    }

    Console.WriteLine();
    Console.WriteLine("Stored credentials:");

    for (int i = 0; i < credentials.Count; i++)
    {
        Console.WriteLine(
            $"{i + 1}. {credentials[i].Service}"
        );

        Console.WriteLine(
            $"   Username: {credentials[i].Username}"
        );

        Console.WriteLine(
            "   Password: ********"
        );
    }
}

static void UpdateCredential(VaultService vaultService)
{
    List<Credential> credentials =
        vaultService.GetCredentials();

    if (credentials.Count == 0)
    {
        Console.WriteLine("No credentials stored.");
        return;
    }

    ListCredentials(vaultService);

    Console.Write("Credential number to update: ");

    if (!int.TryParse(
        Console.ReadLine(),
        out int number))
    {
        Console.WriteLine("Invalid number.");
        return;
    }

    int index = number - 1;

    Console.Write("New service: ");
    string service = Console.ReadLine() ?? string.Empty;

    Console.Write("New username: ");
    string username = Console.ReadLine() ?? string.Empty;

    Console.Write("New password: ");
    string password = ReadPassword();

    bool updated =
        vaultService.UpdateCredential(
            index,
            service,
            username,
            password
        );

    if (updated)
    {
        Console.WriteLine(
            "Credential updated successfully."
        );
    }
    else
    {
        Console.WriteLine(
            "Credential not found."
        );
    }
}

static void DeleteCredential(VaultService vaultService)
{
    List<Credential> credentials =
        vaultService.GetCredentials();

    if (credentials.Count == 0)
    {
        Console.WriteLine("No credentials stored.");
        return;
    }

    ListCredentials(vaultService);

    Console.Write("Credential number to delete: ");

    if (!int.TryParse(
        Console.ReadLine(),
        out int number))
    {
        Console.WriteLine("Invalid number.");
        return;
    }

    int index = number - 1;

    bool deleted =
        vaultService.DeleteCredential(index);

    if (deleted)
    {
        Console.WriteLine(
            "Credential deleted successfully."
        );
    }
    else
    {
        Console.WriteLine(
            "Credential not found."
        );
    }
}

static string ReadPassword()
{
    string password = string.Empty;

    while (true)
    {
        ConsoleKeyInfo key = Console.ReadKey(intercept: true);

        if (key.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            break;
        }

        if (key.Key == ConsoleKey.Backspace)
        {
            if (password.Length > 0)
            {
                password = password[..^1];
            }

            continue;
        }

        if (!char.IsControl(key.KeyChar))
        {
            password += key.KeyChar;
        }
    }

    return password;
}