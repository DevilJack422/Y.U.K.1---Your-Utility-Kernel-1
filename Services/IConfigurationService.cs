using YUK1.Models;

namespace YUK1.Services;

public interface IConfigurationService
{
    AppSettings Settings { get; }

    public void Reload(); // Read config file and run validation
}
