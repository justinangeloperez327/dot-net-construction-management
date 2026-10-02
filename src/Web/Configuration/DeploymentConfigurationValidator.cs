using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Web.Configuration;

public static class DeploymentConfigurationValidator
{
    public static void Validate(
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            return;
        }

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(
                configuration.GetConnectionString("Database")))
        {
            errors.Add(
                "ConnectionStrings:Database must be configured.");
        }

        var allowedHosts = ParseHosts(
            configuration["AllowedHosts"]);

        if (allowedHosts.Length == 0)
        {
            errors.Add(
                "AllowedHosts must contain at least one explicit host.");
        }
        else if (allowedHosts.Contains(
                     "*",
                     StringComparer.Ordinal))
        {
            errors.Add(
                "AllowedHosts cannot contain '*' outside Development.");
        }

        if (environment.IsStaging() &&
            allowedHosts.Length > 0 &&
            allowedHosts.All(IsLocalHost))
        {
            errors.Add(
                "Staging AllowedHosts must include the externally reachable staging host.");
        }

        if (environment.IsProduction() &&
            allowedHosts.Length > 0 &&
            allowedHosts.All(IsLocalHost))
        {
            errors.Add(
                "Production AllowedHosts must include the externally reachable production host.");
        }

        ValidateAbsolutePersistentPath(
            configuration["FileStorage:RootPath"],
            "FileStorage:RootPath",
            errors);

        ValidateAbsolutePersistentPath(
            configuration["Security:DataProtection:KeysPath"],
            "Security:DataProtection:KeysPath",
            errors);

        if (errors.Count == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            "Invalid deployment configuration: " +
            string.Join(" ", errors));
    }

    private static string[] ParseHosts(string? value)
    {
        return value?
            .Split(
                ';',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
            ?? [];
    }

    private static bool IsLocalHost(string host)
    {
        return host.Equals(
                   "localhost",
                   StringComparison.OrdinalIgnoreCase) ||
               host.Equals(
                   "127.0.0.1",
                   StringComparison.Ordinal) ||
               host.Equals(
                   "::1",
                   StringComparison.Ordinal);
    }

    private static void ValidateAbsolutePersistentPath(
        string? value,
        string key,
        ICollection<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(
                $"{key} must be configured.");
            return;
        }

        if (!Path.IsPathRooted(value))
        {
            errors.Add(
                $"{key} must be an absolute persistent path outside Development.");
        }
    }
}
