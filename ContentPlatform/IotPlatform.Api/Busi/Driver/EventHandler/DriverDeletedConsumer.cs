using IotPlatform.Api.Repository;
using Contracts;
using IotPlatform.Api.Busi.Logic.EdgeDriver;
using IotPlatform.Api.Database;
using IotPlatform.Api.Entities;
using IotPlatform.Api.Repository.Driver;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace IotPlatform.Api.Busi.Driver.EventHandler;

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