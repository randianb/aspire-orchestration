using ContentPlatform.Api.Database;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ContentPlatform.Api.Busi.Driver.EventHandler;

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