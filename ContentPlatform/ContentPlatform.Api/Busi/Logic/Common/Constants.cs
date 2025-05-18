using ContentPlatform.Api.Busi.Logic.EdgeDriver;
using ContentPlatform.Api.Busi.Logic.Enums;

namespace ContentPlatform.Api.Busi.Logic.Common;

public static class Constants
{
    public delegate IEdgeDriver EdgeDriverResolver(DriverTypeEnum type);
    public const string EdgedriversPrefix = "edgeDrivers";

}
