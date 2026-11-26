using Microsoft.Extensions.Configuration;
using YUK1.Models;

namespace YUK1.Services;

public class ConfigurationService : IConfigurationService
{
    private const string DefaultOllamaBaseUrl = "http://localhost:11434";
    private readonly string _basePath;

    public AppSettings Settings { get; private set; } = new();

    public ConfigurationService(string? basePath = null)
    {
        _basePath = string.IsNullOrWhiteSpace(basePath) ? AppContext.BaseDirectory : basePath;

        Reload();
    }

    public void Reload()
    {
        var config = new ConfigurationBuilder().SetBasePath(_basePath).AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);

        var settings = new AppSettings();
        config.Build().Bind(settings);

        ValidateAndSanitize(settings);

        Settings = settings;
    }

    private static void ValidateAndSanitize(AppSettings settings)
    {
        settings.Ollama ??= new OllamaSettings();
        settings.HardwareMonitor ??= new HardwareMonitorSettings();
        settings.Widget ??= new WidgetSettings();
        settings.Security ??= new SecuritySettings();

        if (!Uri.TryCreate(settings.Ollama.BaseUrl, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            settings.Ollama.BaseUrl = DefaultOllamaBaseUrl;
        }
        settings.Ollama.TimeoutSeconds = Math.Clamp(settings.Ollama.TimeoutSeconds, 5, 120);

        settings.HardwareMonitor.PollIntervalMs = Math.Clamp(settings.HardwareMonitor.PollIntervalMs, 1000, 5000);
        if (settings.HardwareMonitor.BackgroundPollIntervalMs < 0)
        {
            settings.HardwareMonitor.BackgroundPollIntervalMs = 0;
        }
        else if (settings.HardwareMonitor.BackgroundPollIntervalMs > 0 && settings.HardwareMonitor.BackgroundPollIntervalMs < 5000)
        {
            settings.HardwareMonitor.BackgroundPollIntervalMs = 5000;
        }

        settings.Widget.PopoverAutoCloseSeconds = Math.Max(3, settings.Widget.PopoverAutoCloseSeconds);

        settings.Security.AllowedTools = (settings.Security.AllowedTools ?? new List<string>()).Where(tool => !string.IsNullOrWhiteSpace(tool))
                                                                                                .Select(tool => tool.Trim())
                                                                                                .Distinct(StringComparer.OrdinalIgnoreCase)
                                                                                                .ToList();
    }
}
