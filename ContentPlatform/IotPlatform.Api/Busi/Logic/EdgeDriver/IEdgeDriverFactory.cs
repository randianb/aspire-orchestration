namespace IotPlatform.Api.Busi.Logic.EdgeDriver;

public interface IEdgeDriverFactory
{
    public abstract ObservableDictionary<string, IEdgeDriver> GetDrivers();
}