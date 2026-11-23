using Microsoft.Extensions.Configuration;

namespace YUK1.Configuration;

public sealed class AppSettings
{
    public OllamaSettings Ollama { get; set; } = new();
    public SystemMonitorSettings SystemMonitor { get; set; } = new();
    public UiSettings UI { get; set; } = new();

    /// <summary>
    /// Read configuration from appsettings.json
    /// </summary>
    public static AppSettings Load()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .Build();

        var settings = new AppSettings();
        config.Bind(settings);
        return settings;
    }
}

public sealed class OllamaSettings
{
    public string Endpoint { get; set; } = "http://localhost:11434";
    public string Model { get; set; } = "qwen2.5:3b";
    public string KeepAlive { get; set; } = "5m";
    public int RequestTimeoutSeconds { get; set; } = 60;
}

public sealed class SystemMonitorSettings
{
    public int PollIntervalMs { get; set; } = 2000;
    public int CpuWarningThresholdPercent { get; set; } = 85;
    public int RamWarningThresholdPercent { get; set; } = 85;
}

public sealed class UiSettings
{
    public double WindowWidth { get; set; } = 420;
    public double WindowHeight { get; set; } = 600;
    public string GlobalHotkey { get; set; } = "Alt+Space";
    public bool StartWithWindows { get; set; } = false;
}