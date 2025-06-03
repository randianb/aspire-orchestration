using IotPlatform.Api.Repository;
using Contracts;
using IotPlatform.Api.Busi.Logic.EdgeDriver;
using IotPlatform.Api.Database;
using IotPlatform.Api.Entities;
using IotPlatform.Api.Repository.Driver;
using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace IotPlatform.Api.Busi.Tag.EventHandler;

public sealed class TagCreatedConsumer : IConsumer<TagCreatedEvent>
{
    private readonly ApplicationDbContext _context;
    IDriverRepository _iDriverRepository;
    private  IEdgeDriverFactory edgeDriverFactory;
    public TagCreatedConsumer(ApplicationDbContext context, IDriverRepository iDriverRepository, IEdgeDriverFactory edgeDriverFactory)
    {
        _context = context;
        _iDriverRepository = iDriverRepository;
        this.edgeDriverFactory = edgeDriverFactory;
    }

    public async Task Consume(ConsumeContext<TagCreatedEvent> context)
    {
        TagEntity tag =context.Message.Adapt<TagEntity>();
        if (tag.DriverCode is not null)
        {
            var driver = await _iDriverRepository.GetQuery().FirstOrDefaultAsync(d => d.DriverCode == tag.DriverCode);
            //根据这个DriverConf 获取 Dirver
            if (driver is not null && edgeDriverFactory.GetDrivers().ContainsKey(driver.DriverCode))
            {
                edgeDriverFactory.GetDrivers()[driver.DriverCode].DoRead([tag]);
            }
        }
    }
}