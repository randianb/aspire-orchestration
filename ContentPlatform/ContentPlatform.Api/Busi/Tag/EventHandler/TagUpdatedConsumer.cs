using ContentPlatform.Api.Database;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ContentPlatform.Api.Busi.Tag.EventHandler;

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