using Contracts;
using IotPlatform.Api.Entities;
using IotPlatform.Api.Repository.Channel;
using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace IotPlatform.Api.Busi.Tag.EventHandler;

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