using IotPlatform.Api.Entities;
using Contracts;
using IotPlatform.Api.Database;
using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace IotPlatform.Api.Busi.Sender.EventHandler;

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