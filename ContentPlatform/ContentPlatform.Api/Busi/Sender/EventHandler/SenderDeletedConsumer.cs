using ContentPlatform.Api.Database;
using ContentPlatform.Api.Entities;
using Contracts;
using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ContentPlatform.Api.Busi.Sender.EventHandler;

public sealed class SenderDeletedConsumer : IConsumer<SenderDeletedEvent>
{
    private readonly ApplicationDbContext _context;

    public SenderDeletedConsumer(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<SenderDeletedEvent> context)
    {
       
    }
}