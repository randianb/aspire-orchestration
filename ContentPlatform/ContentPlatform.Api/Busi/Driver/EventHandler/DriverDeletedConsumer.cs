using ContentPlatform.Api.Busi.Logic.EdgeDriver;
using ContentPlatform.Api.Database;
using ContentPlatform.Api.Entities;
using ContentPlatform.Api.Repository;
using ContentPlatform.Api.Repository.Driver;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ContentPlatform.Api.Busi.Driver.EventHandler;

public sealed class DriverDeletedConsumer : IConsumer<DriverDeletedEvent>
{
    private readonly ApplicationDbContext _context;
    private IDriverRepository driverRepository;
    private  IEdgeDriverFactory edgeDriverFactory;
    public DriverDeletedConsumer(ApplicationDbContext context,IDriverRepository driverRepository,  IEdgeDriverFactory edgeDriverFactory)
    {
        _context = context;
        this.driverRepository = driverRepository;
        this.edgeDriverFactory = edgeDriverFactory;
    }

    public async Task Consume(ConsumeContext<DriverDeletedEvent> context)
    {
        DriverEntity driver = await driverRepository.GetQuery().FirstOrDefaultAsync(x => x.Id == context.Message.Id);
        if (driver is not null )
        {
            edgeDriverFactory.GetDrivers()[driver.DriverCode].Stop();
            edgeDriverFactory.GetDrivers().Remove(driver.DriverCode);    
        }
    }
}