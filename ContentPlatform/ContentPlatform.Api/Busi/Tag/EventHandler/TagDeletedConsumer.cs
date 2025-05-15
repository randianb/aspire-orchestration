using ContentPlatform.Api.Entities;
using ContentPlatform.Api.Repository.Channel;
using Contracts;
using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ContentPlatform.Api.Busi.Tag.EventHandler;

public sealed class TagDeletedConsumer(IChannelTagRepository channelTagRepository) : IConsumer<TagDeletedEvent>
{
    public async Task Consume(ConsumeContext<TagDeletedEvent> context)
    {
        TagEntity tag = context.Message.Adapt<TagEntity>();
        var channels = await channelTagRepository.GetQuery().Where(x => x.TagCode == tag.TagCode).ToListAsync();
        foreach (var channelTagEntity in channels)
        {
            channelTagRepository.DeleteEntity(channelTagEntity);
        }
    }
}