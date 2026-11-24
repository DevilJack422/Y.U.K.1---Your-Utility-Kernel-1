using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace YUK1.Services;

/// <summary>
/// Register global hotkey throught Win32 API
/// </summary>
public class HotkeyService
{
    private const int WnHotkey = 0x0312;
    private const int HotkeyId = 9001;

    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint ModShift = 0x0004;
    private const uint ModWin = 0x0008;
    private const uint ModNoRepeat = 0x4000;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(nint hWnd, int id);

    private HwndSource? _hwndSource;
    private nint _windowHandle;
    private bool _isRegistered;
    private bool _disposed;

    public event Action? HotkeyPressed;

    /// <summary>
    /// Register hotkey using the config string in appsetting.json
    /// </summary>
    public bool Register(Window window, string hotkeyConfig)
    {
        var (modifiers, key) = ParseHotkey(hotkeyConfig);
        uint vk = (uint)KeyInterop.VirtualKeyFromKey(key);

        _windowHandle = new WindowInteropHelper(window).EnsureHandle();
        _hwndSource = HwndSource.FromHwnd(_windowHandle);
        _hwndSource?.AddHook(WndProc);

        _isRegistered = RegisterHotKey(_windowHandle, HotkeyId, modifiers | ModNoRepeat, vk);
        return _isRegistered;
    }

    private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        if (msg == WnHotkey && wParam.ToInt32() == HotkeyId)
        {
            HotkeyPressed?.Invoke();
            handled = true;
        }
        return nint.Zero;
    }

    private static (uint Modifiers, Key key) ParseHotkey(string hotkeyConfig)
    {
        uint modifiers = 0;
        Key key = Key.Space;

        if (string.IsNullOrWhiteSpace(hotkeyConfig)) return (ModAlt, Key.Space);

        var parts = hotkeyConfig.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var part in parts)
        {
            switch (part.ToUpperInvariant())
            {
                case "ALT":
                    modifiers |= ModAlt;
                    break;

                case "CTRL":

                case "CONTROL":
                    modifiers |= ModControl;
                    break;

                case "SHIFT":
                    modifiers |= ModShift;
                    break;

                case "WIN":
                    modifiers |= ModWin;
                    break;

                default:
                    if (Enum.TryParse<Key>(part, ignoreCase: true, out var parseKey))
                        key = parseKey;
                    break;
            }
        }

        return (modifiers == 0 ? ModAlt : modifiers, key);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_isRegistered && _windowHandle != nint.Zero)
        {
            UnregisterHotKey(_windowHandle, HotkeyId);
            _isRegistered = false;
        }

        _hwndSource?.RemoveHook(WndProc);
        _hwndSource = null;
    }
}
