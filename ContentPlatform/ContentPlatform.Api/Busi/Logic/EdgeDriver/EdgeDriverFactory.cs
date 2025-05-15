namespace ContentPlatform.Api.Busi.Logic.EdgeDriver;

public class EdgeDriverFactory(): IEdgeDriverFactory
{
    private Dictionary<String, IEdgeDriver> EdgeDrivers { get; set; } = new Dictionary<String, IEdgeDriver>();
    public Dictionary<string, IEdgeDriver> GetDrivers()
    {
        return EdgeDrivers;
    }
}