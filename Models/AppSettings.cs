namespace YUK1.Models;

public class AppSettings
{
    public OllamaSettings Ollama { get; set; } = new();
    public HardwareMonitorSettings HardwareMonitor { get; set; } = new();
    public WidgetSettings Widget { get; set; } = new();
    public SecuritySettings Security { get; set; } = new();
}

public class OllamaSettings
{
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string ModelName { get; set; } = "qwen2.5:3b";
    public string KeepAlive { get; set; } = "5m";
    public int TimeoutSeconds { get; set; } = 60; 
}

public class HardwareMonitorSettings
{
    public int PollIntervalMs { get; set; } = 1500;
    public int BackgroundPollIntervalMs { get; set; } = 5000;
}

public class WidgetSettings
{
    public string Hotkey { get; set; } = "Alt+Space";
    public int PopoverAutoCloseSeconds { get; set; } = 10;
    public bool AutoStartWithWindows { get; set; } = true;
}

public class SecuritySettings
{
    public List<string> AllowedTools { get; set; } = new();
    public bool RequireConfirmation { get; set; } = true;
}