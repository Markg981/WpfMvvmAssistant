namespace Core.Domain.Models;

public class ConnectionInfo
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string ServerName { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public bool UseWindowsAuth { get; set; } = true;
    public string? Username { get; set; }
    public byte[]? EncryptedPassword { get; set; }
    public int ConnectionTimeout { get; set; } = 30;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsed { get; set; }

    public string GetConnectionString()
    {
        var builder = new System.Data.SqlClient.SqlConnectionStringBuilder
        {
            DataSource = ServerName,
            InitialCatalog = DatabaseName,
            ConnectTimeout = ConnectionTimeout,
            TrustServerCertificate = true
        };

        if (UseWindowsAuth)
        {
            builder.IntegratedSecurity = true;
        }
        else
        {
            builder.UserID = Username;
            builder.Password = DecryptPassword();
        }

        return builder.ConnectionString;
    }

    private string? DecryptPassword()
    {
        if (EncryptedPassword == null || EncryptedPassword.Length == 0)
            return null;

        try
        {
            var decrypted = System.Security.Cryptography.ProtectedData.Unprotect(
                EncryptedPassword,
                null,
                System.Security.Cryptography.DataProtectionScope.CurrentUser);
            return System.Text.Encoding.UTF8.GetString(decrypted);
        }
        catch
        {
            return null;
        }
    }

    public void SetPassword(string? password)
    {
        if (string.IsNullOrEmpty(password))
        {
            EncryptedPassword = null;
            return;
        }

        var data = System.Text.Encoding.UTF8.GetBytes(password);
        EncryptedPassword = System.Security.Cryptography.ProtectedData.Protect(
            data,
            null,
            System.Security.Cryptography.DataProtectionScope.CurrentUser);
    }
}
