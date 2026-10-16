using System.Diagnostics;
using System.IO;
using System.Security;
using Microsoft.Win32;
using WordFlash.Core;

namespace WordFlash.App;

/// <summary>
/// Adds or removes the per-user "Run" registry entry that starts Word-flash at sign-in.
/// </summary>
internal static class StartupRegistration
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "WordFlash";

    /// <summary>
    /// Brings the registry entry in line with <paramref name="enabled"/>. Re-run at every launch so the
    /// stored path follows the executable if it has been moved. Returns false if the registry refused.
    /// </summary>
    public static bool Apply(bool enabled)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);
            if (!enabled)
            {
                key.DeleteValue(ValueName, throwOnMissingValue: false);
                return true;
            }

            var exePath = Environment.ProcessPath
                ?? throw new InvalidOperationException("The executable path is unknown.");
            if (!StartupCommand.Matches(key.GetValue(ValueName) as string, exePath))
            {
                key.SetValue(ValueName, StartupCommand.Build(exePath), RegistryValueKind.String);
            }

            return true;
        }
        catch (Exception ex) when (ex is SecurityException or UnauthorizedAccessException or IOException or ArgumentException or InvalidOperationException)
        {
            Debug.WriteLine($"WordFlash: could not update the startup entry: {ex.Message}");
            return false;
        }
    }
}
