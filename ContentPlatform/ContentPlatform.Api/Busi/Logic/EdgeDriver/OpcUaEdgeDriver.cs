// Make an URL of OPC DA server using builder.

using System.Diagnostics;
using ContentPlatform.Api.Entities;
using ContentPlatform.Api.Repository.Tag;
using Opc.Ua;
using OPCUaClient;

namespace ContentPlatform.Api.Busi.Logic.EdgeDriver;

public class OpcUaEdgeDriver(
    ILogger<OpcUaEdgeDriver> logger,
    IServiceScopeFactory _scopeFactory) : IEdgeDriver
{
    private UaClient client;
    private Timer _timer;
    private Timer _timer1;
    public string DriverCode => _driverConfig.DriverCode;
    private DriverEntity _driverConfig;

    public string ActivitySourceName => _driverConfig.ServerName + typeof(OpcUaEdgeDriver).Name;
    private ActivitySource SActivitySource => new(ActivitySourceName);

    public void Run(DriverEntity driverConfig)
    {
        _driverConfig = driverConfig;
        using var activity =
            SActivitySource.StartActivity(name: typeof(OpcUaEdgeDriver).Name + " Run", ActivityKind.Client);
        if (driverConfig.HasIdentity)
        {
            client = new UaClient(driverConfig.ServerName, driverConfig.ServerUrl, false, true, driverConfig.UserName,
                driverConfig.PassWord);
        }
        else
        {
            client = new UaClient(driverConfig.ServerName, driverConfig.ServerUrl, false, false);
        }

        _timer = new Timer(Monitoring, null, TimeSpan.Zero, Timeout.InfiniteTimeSpan);
        _timer1 = new Timer(CheckConnected, null, TimeSpan.Zero, Timeout.InfiniteTimeSpan);
    }

    public void Stop()
    {
        Dispose();
    }

    private void CheckConnected(object? state)
    {
        while (true)
        {
            Thread.Sleep(10 * 1000);
            try
            {
                if (!client.IsConnected)
                {
                    Monitoring(null);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        _timer?.Dispose(); //如果_timer对象不为null，则销毁
        _timer1?.Dispose(); //如果_timer对象不为null，则销毁
        client?.Disconnect();
    }

    private void Monitoring(object? state)
    {
        try
        {
            // Connect to the server first.
            uint timeOut = 30;
            client.Connect(timeOut);
            using var scope = _scopeFactory.CreateScope();
            // Create a group with items.
            var tags = scope.ServiceProvider.GetRequiredService<ITagRepository>().GetQuery()
                .Where(x => x.DriverCode == _driverConfig.DriverCode).ToList();
            if (tags.Count > 0)
            {
                var address = tags.Select(x => x.TagCode).ToList();

                //     new List<String>
                // {
                //     "通道 1.设备 1.标记 1",
                //     "模拟器示例.函数.Ramp1",
                //     "模拟器示例.函数.Ramp2",
                // };

                foreach (var se in address)
                {
                    client.Monitoring(se, 200, async (_, e) =>
                    {
                        var value = (MonitoredItemNotification)e.NotificationValue;
                        await RunConcurrentTagUpdateAsync(se, value.Value.ToString());
                        logger.LogInformation("Driver: {4}； ItemId: {0}; Value: {1}; Quality: {2}; Timestamp: {3}",
                            _driverConfig.DriverCode, se, value.Value.ToString(), "Good", DateTime.UtcNow);
                    });
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error monitoring");
            CheckConnected(null);
        }
    }

    private async Task RunConcurrentTagUpdateAsync(string se, String value)
    {
        // 使用 _scopeFactory 来创建作用域
        using (var scope = _scopeFactory.CreateScope())
        {
            // *** 在新的作用域内解析你需要 Services/Repositories ***
            // DI 容器会为这个新的作用域创建一个新的 DbContext 实例
            var tagService = scope.ServiceProvider.GetRequiredService<TagService>();

            try
            {
                await tagService.AddOrUpdateTagAsync(se, value);
            }
            catch (Exception ex)
            {
                // 处理异常
                Console.Error.WriteLine($"Error processing tag update: {ex.Message}");
                // 根据需要决定是否重试或记录错误
            }
        }
    }

    public async void DoRead(List<TagEntity> tags)
    {
        try
        {
            var items = tags.Select(tag => tag.TagCode).ToList();
            if (client == null || !client.IsConnected)
            {
                return;
            }

            var values = client.Read(items);
            foreach (var value in values)
            {
                try
                {
                    if (value.Quality)
                    {
                        await RunConcurrentTagUpdateAsync(value.Address, value.Value.ToString());
                        logger.LogInformation("Driver: {4}； ItemId: {0}; Value: {1}; Quality: {2}; Timestamp: {3}",
                            _driverConfig.DriverCode, value.Address, value.Value, value.Quality, DateTime.UtcNow);
                    }
                    else
                    {
                        logger.LogInformation("Driver: {4}； ItemId: {0}; Value: {1}; Quality: {2}; Timestamp: {3}",
                            _driverConfig.DriverCode, value.Address, value.Value, value.Quality, DateTime.UtcNow);
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e.InnerException, "Error processing tag update");
                }

            }
        }
        catch (Exception e)
        {
            logger.LogError(e.InnerException, "Error processing tag update");
        }
    }

    public void DoWrite(List<TagEntity> tags)
    {
        try
        {
            var items = tags.Select(tag => new OPCUaClient.Objects.Tag
            {
                Address = tag.TagCode,
                Value = tag.Value,
            }).ToList();

            client.Write(items);
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
        }
    }
}