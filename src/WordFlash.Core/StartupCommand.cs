namespace WordFlash.Core;

/// <summary>
/// Builds the command line stored under the Windows "Run" registry key.
/// </summary>
public static class StartupCommand
{
    public static string Build(string exePath)
    {
        if (string.IsNullOrWhiteSpace(exePath)
            || !Path.IsPathFullyQualified(exePath)
            || exePath.Contains('"')
            || !exePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Startup path must be a fully qualified .exe path without quotes: '{exePath}'.", nameof(exePath));
        }

        return $"\"{exePath}\"";
    }

    public static bool Matches(string? registryValue, string exePath) =>
        registryValue is not null
        && string.Equals(registryValue, $"\"{exePath}\"", StringComparison.OrdinalIgnoreCase);
}
