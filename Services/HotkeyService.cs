using System.Windows.Input;
using NHotkey;
using NHotkey.Wpf;

namespace YUK1.Services;

/// <summary>
/// Register global hotkey by NHotkey.Wpf library
/// </summary>
public class HotkeyService
{
    private const string HotkeyName = "YUK1_ToggleWindow";
    private bool _disposed;

    public event Action? HotkeyPressed;

    /// <summary>
    /// Register hotkey using the config string in appsetting.json
    /// Return false if key is being holded by other apps
    /// </summary>
    public bool Register(string hotkeyConfig)
    {
        var (modifiers, key) = ParseHotkey(hotkeyConfig);

        try
        {
            HotkeyManager.Current.AddOrReplace(HotkeyName, key, modifiers, OnHotkeyPressed);
            return true;
        }
        catch (HotkeyAlreadyRegisteredException)
        {
            return false;
        }
    }

    private void OnHotkeyPressed(object? sender, HotkeyEventArgs e)
    {
        e.Handled = true;
        HotkeyPressed?.Invoke();
    }

    private static (ModifierKeys Modifiers, Key Key) ParseHotkey(string hotkeyConfig)
    {
        var modifiers = ModifierKeys.None;
        var key = Key.Space;

        if (string.IsNullOrWhiteSpace(hotkeyConfig))
            return (ModifierKeys.Alt, Key.Space);

        var parts = hotkeyConfig.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var part in parts)
        {
            switch (part.ToUpperInvariant())
            {
                case "ALT":
                    modifiers |= ModifierKeys.Alt;
                    break;

                case "CTRL":

                case "CONTROL":
                    modifiers |= ModifierKeys.Control;
                    break;

                case "SHIFT":
                    modifiers |= ModifierKeys.Shift;
                    break;

                case "WIN":
                    modifiers |= ModifierKeys.Windows;
                    break;

                default:
                    if (Enum.TryParse<Key>(part, ignoreCase: true, out var parseKey))
                        key = parseKey;
                    break;
            }
        }

        return (modifiers == ModifierKeys.None ? ModifierKeys.Alt : modifiers, key);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        HotkeyManager.Current.Remove(HotkeyName);
    }
}
