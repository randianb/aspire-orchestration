using ContentPlatform.Api.Busi.Logic.EdgeDriver;
using ContentPlatform.Api.Database;
using ContentPlatform.Api.Entities;
using ContentPlatform.Api.Repository;
using ContentPlatform.Api.Repository.Driver;
using Contracts;
using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ContentPlatform.Api.Busi.Tag.EventHandler;

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