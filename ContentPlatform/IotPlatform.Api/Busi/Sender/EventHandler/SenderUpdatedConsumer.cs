using Contracts;
using IotPlatform.Api.Database;
using MassTransit;

namespace IotPlatform.Api.Busi.Driver.EventHandler;

public sealed class SenderUpdatedConsumer : IConsumer<DriverUpdatedEvent>
{
    private readonly ApplicationDbContext _context;

    public SenderUpdatedConsumer(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<DriverUpdatedEvent> context)
    {
        
    }
}