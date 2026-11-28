using System.Windows.Input;
using NHotkey;
using NHotkey.Wpf;

namespace YUK1.Services;

public class HotkeyService : IHotkeyService
{
    private const string HotkeyName = "YUK1_ToggleWidget";
    private Action? _callback;

    public bool IsRegistered { get; private set; }

    public bool Register(string hotkeyExpression, Action callback)
    {
        if (callback == null) return false;
        
        if (!TryParseHotkey(hotkeyExpression, out ModifierKeys modifiers, out Key key))
        {
            modifiers = ModifierKeys.Alt;
            key = Key.Space;
        }
        _callback = callback;

        try
        {
            HotkeyManager.Current.AddOrReplace(HotkeyName, key, modifiers, OnHotkeyPressed);
            IsRegistered = true;
            return true;
        } 
        catch (HotkeyAlreadyRegisteredException)
        {
            IsRegistered = true;
            return false;
        }
    }

    public void Unregister()
    {
        if (IsRegistered == false) return;

        HotkeyManager.Current.Remove(HotkeyName);
        IsRegistered = false;
        _callback = null;
    }

    public void Dispose()
    {
        Unregister();
    }

    private void OnHotkeyPressed(object? sender, HotkeyEventArgs e)
    {
        e.Handled = true;
        if (sender != null) _callback?.Invoke();
    }

    private static bool TryParseHotkey(string expression, out ModifierKeys modifiers, out Key key)
    {
        modifiers = ModifierKeys.None;
        key = Key.None;

        if (string.IsNullOrWhiteSpace(expression)) return false;

        var tokens = expression.Split("+", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (tokens.Length == 0) return false;

        foreach (var token in tokens)
        {
            switch (token.ToUpperInvariant())
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
                case "WINDOWS":
                    modifiers |= ModifierKeys.Windows;
                    break;

                default:
                    if (!Enum.TryParse<Key>(token, ignoreCase: true, out key))
                    {
                        return false;
                    }
                    break;
            }
        }
        
        return key != Key.None;
    }
}
