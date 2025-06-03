using Contracts;
using IotPlatform.Api.Busi.Tag.Hubs;
using IotPlatform.Api.DataMap;
using IotPlatform.Api.Entities;
using IotPlatform.Api.Repository.Channel;
using IotPlatform.Api.Repository.Tag;
using Mapster;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace IotPlatform.Api.Busi.Tag.EventHandler;

public class TagChangeConsumer(
    IChannelRepository channelRepository,
    IChannelTagRepository channelTagRepository,
    ITagRepository tagRepository,
    IHubContext<TagNotificationHub,ITagNotificationClient> hubContext,
    IPublishEndpoint _publishEndpoint) :
    IConsumer<TagCreatedEvent>,
    IConsumer<TagValueUpdatedEvent>,
    IConsumer<TagUpdatedEvent>
{
    public async Task Consume(ConsumeContext<TagCreatedEvent> context)
    {
        await ChannelReadCommon(context);
    }

    private async Task ChannelReadCommon(ConsumeContext<TagCreatedEvent> context)
    {
        var tag = await tagRepository.GetQuery().FirstOrDefaultAsync(x => x.TagCode == context.Message.TagCode);
        await ChannelRead(tag);
    }

    public async Task Consume(ConsumeContext<TagValueUpdatedEvent> context)
    {
        var tag = await tagRepository.GetQuery().FirstOrDefaultAsync(x => x.TagCode == context.Message.TagCode);
        await ChannelRead(tag);
    }

    public async Task Consume(ConsumeContext<TagUpdatedEvent> context)
    {
        var tag = await tagRepository.GetQuery().FirstOrDefaultAsync(x => x.TagCode == context.Message.TagCode);
        await ChannelRead(tag);
    }

    public async Task ChannelRead(TagEntity tag)
    {
        await hubContext.Clients.All.SendTagValueUpdate(tag);
        var channels = await channelTagRepository.GetQuery(true).Where(x => x.TagCode == tag.TagCode).ToListAsync();
        if (channels is not null)
        {
            foreach (var channelTagEntity in channels)
            {
                channelTagEntity.Value = tag.Value;
                channelTagEntity.UpdateTime = tag.UpdateTime;
                channelTagEntity.LastValue = tag.LastValue;
                channelTagEntity.LastUpdateTime = tag.LastUpdateTime;
                await channelTagRepository.SaveChangesAsync();
                var channel = await channelRepository.GetQuery(true)
                    .FirstOrDefaultAsync(x => x.ChannelCode == channelTagEntity.ChannelCode);

                //实时发送全部
                if (!channel.IsSchedule)
                {
                    var channelTags = await channelTagRepository.GetQuery()
                        .Where(x => x.ChannelCode == channelTagEntity.ChannelCode && x.TagCode == tag.TagCode)
                        .ToListAsync();
                    var tags = ChannelTagMap.ChannelTagDtos(channelTags);
                    await _publishEndpoint.Publish(
                        new ChannelTagChangedEvent(tags, channel.ChannelCode, channel.SenderCodes));
                }
            }
        }
    }
}