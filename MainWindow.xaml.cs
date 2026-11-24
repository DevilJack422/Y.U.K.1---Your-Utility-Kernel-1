using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using H.NotifyIcon.Core;
using YUK1.Configuration;
using YUK1.Services;

namespace YUK1;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly AppSettings _settings;
    private readonly HotkeyService _hotkeyService;
    private bool _isExisting;

    public MainWindow()
    {
        InitializeComponent();

        _settings = AppSettings.Load();
        Width = _settings.UI.WindowWidth;
        Height = _settings.UI.WindowHeight;
        ModelStatusText.Text = $"{_settings.Ollama.Model} (Not load)";

        AutoStartMenuItem.IsChecked = AutoStartService.IsEnable();

        _hotkeyService = new HotkeyService();
        _hotkeyService.HotkeyPressed += ToggleWindow;
        if (!_hotkeyService.Register(_settings.UI.GlobalHotkey))
        {
            ModelStatusText.Text = $"{_settings.Ollama.Model} (Hotkeys are occuppied)";
        }
    }

    private void ToggleWindow()
    {
        if (IsVisible)
        {
            Hide();
        }
        else
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
            PromptInput.Focus();
        }
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Hide();
    private void TrayIcon_LeftClick(object sender, RoutedEventArgs e) => ToggleWindow();
    private void ToggleWindow_Click(object sender, RoutedEventArgs e) => ToggleWindow();

    private void AutoStartMenuItem_Click(object sender, RoutedEventArgs e)
    {
        AutoStartService.SetEnable(AutoStartMenuItem.IsChecked);
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        _isExisting = true;
        _hotkeyService.Dispose();
        TrayIcon.Dispose();
        Application.Current.Shutdown();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_isExisting)
        {
            e.Cancel = true;
            Hide();
        }
        base.OnClosing(e);
    }
}