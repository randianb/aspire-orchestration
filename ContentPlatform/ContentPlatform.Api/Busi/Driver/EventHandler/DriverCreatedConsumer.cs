using ContentPlatform.Api.Busi.Logic.Common;
using ContentPlatform.Api.Busi.Logic.EdgeDriver;
using ContentPlatform.Api.Busi.Logic.Enums;
using ContentPlatform.Api.Database;
using ContentPlatform.Api.Entities;
using Contracts;
using Mapster;
using MassTransit;

namespace ContentPlatform.Api.Busi.Driver.EventHandler;

public sealed class DriverCreatedConsumer : IConsumer<DriverCreatedEvent>
{
    private readonly ApplicationDbContext _context;
    private OpcUaEdgeDriver _opcUaEdgeDriver;
    private Constants.EdgeDriverResolver  edgeDriverResolver;
    private  IEdgeDriverFactory edgeDriverFactory;
    public DriverCreatedConsumer(ApplicationDbContext context, Constants.EdgeDriverResolver edgeDriverResolver, IEdgeDriverFactory edgeDriverFactory)
    {
        _context = context;
        this.edgeDriverResolver = edgeDriverResolver;
        this.edgeDriverFactory = edgeDriverFactory;
    }

    public async Task Consume(ConsumeContext<DriverCreatedEvent> context)
    {
       //创建一个驱动运行即可，附带绑定的tag
       DriverEntity driver =context.Message.Adapt<DriverEntity>();
       var edgeDriver = edgeDriverResolver((DriverTypeEnum)driver.DriverType);
       edgeDriverFactory.GetDrivers().Add(driver.DriverCode, edgeDriver);
       edgeDriver.Run(driver);
    }
}