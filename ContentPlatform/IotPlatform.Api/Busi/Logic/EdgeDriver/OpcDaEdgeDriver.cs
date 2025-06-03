using System.Diagnostics;
using IotPlatform.Api.Entities;
using TitaniumAS.Opc.Client.Common;
using TitaniumAS.Opc.Client.Da;

namespace IotPlatform.Api.Busi.Logic.EdgeDriver;

public class OpcDaEdgeDriver(ILogger<OpcDaEdgeDriver> logger,  TagService tagService,IServiceScopeFactory _scopeFactory) : IEdgeDriver
{
    private OpcDaServer server;
    private Timer _timer;
    private Timer _timer1;
    private DriverEntity _driverConfig;
    public string DriverCode => _driverConfig.DriverCode;
    public string ActivitySourceName => _driverConfig.ServerName + typeof(OpcDaEdgeDriver).Name;
    private ActivitySource SActivitySource => new(ActivitySourceName);

    public void Run(DriverEntity driverConfig)
    {
        _driverConfig = driverConfig;
        //"Matrikon.OPC.Simulation.1"
        using var activity = SActivitySource.StartActivity(typeof(OpcDaEdgeDriver).Name + " Run", ActivityKind.Client);
        Uri url = UrlBuilder.Build(driverConfig.ServerName, driverConfig.ServerUrl);
        server = new OpcDaServer(url);
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
                if (!server.IsConnected)
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

    public void Monitoring(object? state)
    {
        try
        {
            // Connect to the server first.
            server.Connect();
            // Create a group with items.
            OpcDaGroup group = server.AddGroup("MyGroup");
            group.IsActive = true;

            var definition1 = new OpcDaItemDefinition
            {
                ItemId = "Random.Int2",
                IsActive = true
            };
            var definition2 = new OpcDaItemDefinition
            {
                ItemId = "Bucket Brigade.Int4",
                IsActive = true
            };
            OpcDaItemDefinition[] definitions = { definition1, definition2 };
            OpcDaItemResult[] results = group.AddItems(definitions);

            // Configure subscription.
            group.ValuesChanged += OnGroupValuesChanged;
            group.UpdateRate = TimeSpan.FromMilliseconds(100); // ValuesChanged won't be triggered if zero

            async void OnGroupValuesChanged(object sender, OpcDaItemValuesChangedEventArgs args)
            {
                // Output values.
                try
                {
                    foreach (OpcDaItemValue value in args.Values)
                    {
                        await RunConcurrentTagUpdateAsync( value.Item.ItemId,value.Value.ToString());

                        logger.LogInformation("Driver: {4}； ItemId: {0}; Value: {1}; Quality: {2}; Timestamp: {3}",
                            _driverConfig.DriverCode, value.Item.ItemId, value.Value.ToString(), value.Quality, value.Timestamp);
                    }
                }
                catch (Exception e)
                {
                    logger.LogWarning(e, "Error in OnGroupValuesChanged");
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
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

    public void DoWrite(List<TagEntity> tags)
    {
    }


    public void DoRead(List<TagEntity> tags)
    {
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        _timer?.Dispose(); //如果_timer对象不为null，则销毁
        _timer1?.Dispose(); //如果_timer对象不为null，则销毁
        server?.Dispose();
    }
}
