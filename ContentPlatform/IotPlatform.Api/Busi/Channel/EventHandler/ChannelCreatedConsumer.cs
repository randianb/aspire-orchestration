using Contracts;
using IotPlatform.Api.Database;
using IotPlatform.Api.Entities;
using IotPlatform.Api.Repository.Channel;
using IotPlatform.Api.Repository.Tag;
using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace IotPlatform.Api.Busi.Channel.EventHandler;

public sealed class ChannelCreatedConsumer : IConsumer<ChannelCreatedEvent>
{
    private readonly ApplicationDbContext _context;
    private IChannelTagRepository channelTagRepository;
    private ITagRepository tagRepository;

    public ChannelCreatedConsumer(ApplicationDbContext context, IChannelTagRepository channelTagRepository,
        ITagRepository tagRepository)
    {
        _context = context;
        this.channelTagRepository = channelTagRepository;
        this.tagRepository = tagRepository;
    }

    public async Task Consume(ConsumeContext<ChannelCreatedEvent> context)
    {
        try
        {
            ChannelEntity channel = context.Message.Adapt<ChannelEntity>();
            var tags = await tagRepository.GetQuery().Where(x => channel.TagCodes.Contains(x.TagCode)).ToListAsync();
            var channelTags = await channelTagRepository.GetQuery().Where(x => channel.TagCodes.Contains(x.TagCode)).ToListAsync();
            foreach (var channelTagEntity in channelTags)
            {
               await  channelTagRepository.DeleteEntity(channelTagEntity);
            }
            foreach (var tag in tags)
            {
                var channelTag = tag.Adapt<ChannelTagEntity>();
                channelTag.ChannelCode=channel.ChannelCode;
                channelTag.Id=Guid.NewGuid();
               await channelTagRepository.CreateAsync(channelTag);
            }

            await channelTagRepository.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}