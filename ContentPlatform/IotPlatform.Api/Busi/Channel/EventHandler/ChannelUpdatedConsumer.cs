using Contracts;
using IotPlatform.Api.Database;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace IotPlatform.Api.Busi.Driver.EventHandler;

public sealed class ChannelUpdatedConsumer : IConsumer<DriverUpdatedEvent>
{
    private readonly ApplicationDbContext _context;

    public ChannelUpdatedConsumer(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<DriverUpdatedEvent> context)
    {
        
    }
}