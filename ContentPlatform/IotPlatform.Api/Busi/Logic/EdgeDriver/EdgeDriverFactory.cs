using IotPlatform.Api.Busi.Logic.Common;
using Microsoft.Extensions.Caching.Hybrid;
using ZiggyCreatures.Caching.Fusion;

namespace IotPlatform.Api.Busi.Logic.EdgeDriver;

public class EdgeDriverFactory : IEdgeDriverFactory
{

    public EdgeDriverFactory(IFusionCache fusionCache, ObservableDictionary<String, IEdgeDriver> edgeDrivers)
    {
        EdgeDrivers = edgeDrivers;
        EdgeDrivers.ItemAdded += async (sender, e) =>
        {
            //Console.WriteLine($"Item added: Key={e.Key}, Value={e.NewValue}");
            await fusionCache.GetOrSetAsync(Constants.EdgedriversPrefix + e.Key,"running");
        };
        EdgeDrivers.ItemRemoved += (sender, e) =>
        {
            Console.WriteLine($"Item removed: Key={e.Key}, Value={e.NewValue}"); // 注意：这里是移除前的值
            fusionCache.RemoveAsync(Constants.EdgedriversPrefix + e.Key);
        };
    }

    private ObservableDictionary<String, IEdgeDriver> EdgeDrivers { get; set; }

    public ObservableDictionary<string, IEdgeDriver> GetDrivers()
    {
        return EdgeDrivers;
    }
}