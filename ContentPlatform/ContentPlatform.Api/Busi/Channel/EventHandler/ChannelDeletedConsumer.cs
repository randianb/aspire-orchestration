using ContentPlatform.Api.Database;
using ContentPlatform.Api.Entities;
using ContentPlatform.Api.Repository.Channel;
using Contracts;
using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ContentPlatform.Api.Busi.Channel.EventHandler;

public sealed class ChannelDeletedConsumer : IConsumer<ChannelDeletedEvent>
{
    private readonly ApplicationDbContext _context;
    private IChannelTagRepository channelTagRepository;

    public ChannelDeletedConsumer(ApplicationDbContext context, IChannelTagRepository channelTagRepository)
    {
        _context = context;
        this.channelTagRepository = channelTagRepository;
    }

    public async Task Consume(ConsumeContext<ChannelDeletedEvent> context)
    {
        ChannelEntity channel = context.Message.Adapt<ChannelEntity>();
        var channelTags = channelTagRepository.GetQuery().Where(x => channel.TagCodes.Contains(x.TagCode));
        foreach (var channelTagEntity in channelTags)
        {
            channelTagRepository.DeleteEntity(channelTagEntity);
        }
    }
}