using IotPlatform.Api.Busi.Logic.EdgeDriver;
using IotPlatform.Api.Busi.Logic.Enums;

namespace IotPlatform.Api.Busi.Logic.Common;

public static class Constants
{
    public delegate IEdgeDriver EdgeDriverResolver(DriverTypeEnum type);
    public const string EdgedriversPrefix = "edgeDrivers";

}
