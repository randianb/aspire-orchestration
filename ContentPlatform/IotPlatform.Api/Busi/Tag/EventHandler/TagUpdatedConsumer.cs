using Contracts;
using IotPlatform.Api.Database;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace IotPlatform.Api.Busi.Tag.EventHandler;

public sealed class TagUpdatedConsumer : IConsumer<TagUpdatedEvent>
{
    private readonly ApplicationDbContext _context;

    public TagUpdatedConsumer(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<TagUpdatedEvent> context)
    {
        
    }
}