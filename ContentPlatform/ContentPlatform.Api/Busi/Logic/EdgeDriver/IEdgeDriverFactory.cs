namespace ContentPlatform.Api.Busi.Logic.EdgeDriver;

public interface IEdgeDriverFactory
{
    Dictionary<string,IEdgeDriver> GetDrivers();
}