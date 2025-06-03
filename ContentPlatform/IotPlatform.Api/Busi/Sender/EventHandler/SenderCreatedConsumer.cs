using IotPlatform.Api.Entities;
using Contracts;
using IotPlatform.Api.Database;
using IotPlatform.Api.Repository.Tag;
using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace IotPlatform.Api.Busi.Sender.EventHandler;

public sealed class SenderCreatedConsumer : IConsumer<SenderCreatedEvent>
{
    private readonly ApplicationDbContext _context;
    private ITagRepository tagRepository;

    public SenderCreatedConsumer(ApplicationDbContext context, 
        ITagRepository tagRepository)
    {
        _context = context;
        this.tagRepository = tagRepository;
    }

    public async Task Consume(ConsumeContext<SenderCreatedEvent> context)
    {
       
    }
}