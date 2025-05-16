using ContentPlatform.Api.Database;
using ContentPlatform.Api.Entities;
using ContentPlatform.Api.Repository.Tag;
using Contracts;
using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ContentPlatform.Api.Busi.Sender.EventHandler;

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