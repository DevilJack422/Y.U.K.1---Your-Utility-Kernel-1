using System.Net.NetworkInformation;
using Hardware.Info;

namespace YUK1.Services;

public readonly record struct SystemMetrics(double CpuPercent, double RamPercent, double NetworkKbPerSec);

/// <summary>
/// Monitor CPU, RAM, and Network in an asynchronous cycle using Hardware.Info and .NET NetworkInformation.
/// </summary>
public sealed class SystemMonitorService : IDisposable
{
    private readonly IHardwareInfo _hardwareInfo = new HardwareInfo();
    private readonly int _pollIntervalMs;
    private CancellationTokenSource? _cts;
    private Task? _monitorTask;

    private long _lastTotalBytes;
    private DateTime _lastSampleTime;
    private bool _disposed;

    public event Action<SystemMetrics>? MetricsUpdated;
    
    public SystemMonitorService(int pollIntervalMs)
    {
        _pollIntervalMs = Math.Max(500, pollIntervalMs);
    }

    public void Start()
    {
        if (_monitorTask is not null) return;

        _lastTotalBytes = GetTotalNetworkBytes();
        _lastSampleTime = DateTime.UtcNow;
        _cts = new CancellationTokenSource();
        _monitorTask = RunLoopAsync(_cts.Token);
    }

    private async Task RunLoopAsync(CancellationToken token)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMicroseconds(_pollIntervalMs));

        try
        {
            while (await timer.WaitForNextTickAsync(token))
            {
                var metrics = await Task.Run(CollectMetrics, token);
                MetricsUpdated?.Invoke(metrics);
            }
        }
        catch (OperationCanceledException)
        {
            // Stop loop safely when exit app
        }
    }

    private SystemMetrics CollectMetrics()
    {
        _hardwareInfo.RefreshCPUList();
        _hardwareInfo.RefreshMemoryList();

        double cpu = _hardwareInfo.CpuList.FirstOrDefault()?.PercentProcessorTime ?? 0;

        double ram = 0;
        var mem = _hardwareInfo.MemoryStatus;
        if (mem.TotalPhysical > 0)
        {
            ram = (mem.TotalPhysical - mem.AvailablePhysical) * 100.0 / mem.TotalPhysical;
        }

        long currentBytes = GetTotalNetworkBytes();
        var now = DateTime.UtcNow;
        double seconds = Math.Max(0.1, (now - _lastSampleTime).TotalSeconds);
        double netKbPerSec = Math.Max(0, (currentBytes - _lastTotalBytes) / 1024.0 / seconds);

        _lastTotalBytes = currentBytes;
        _lastSampleTime = now;

        return new SystemMetrics(
            Math.Round(cpu, 0),
            Math.Round(ram, 0),
            Math.Round(netKbPerSec, 1));
    }

    private static long GetTotalNetworkBytes()
    {
        long total = 0;
        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (nic.OperationalStatus != OperationalStatus.Up || nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
            {
                continue;
            }

            var stats = nic.GetIPStatistics();
            total += stats.BytesReceived + stats.BytesSent;
        }
        return total;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _cts?.Cancel();
        _cts?.Dispose();
    }
}
