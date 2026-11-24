using Microsoft.Win32;

namespace YUK1.Services;

/// <summary>
/// Manage auto start with Windows throught HKCU Register
/// </summary>
public class AutoStartService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "YUK1";

    public static bool IsEnable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
        return key?.GetValue(AppName) is string value && !string.IsNullOrWhiteSpace(value);
    }

    public static void SetEnable(bool enable)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
        if (key is null) return;

        if (enable)
        {
            var exePath = Environment.ProcessPath;
            if (!string.IsNullOrWhiteSpace(exePath))
            {
                key.SetValue(AppName, $"\"{exePath}\"");
            }
        }   
        else
        {
            key.DeleteValue(AppName, throwOnMissingValue: false);
        }
    }
}
