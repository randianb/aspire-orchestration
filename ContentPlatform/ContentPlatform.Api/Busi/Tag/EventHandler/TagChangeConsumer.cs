using ContentPlatform.Api.Entities;
using ContentPlatform.Api.Repository.Channel;
using ContentPlatform.Api.Repository.Tag;
using Contracts;
using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ContentPlatform.Api.Busi.Tag.EventHandler;

public class TagChangeConsumer(
    IChannelRepository channelRepository,
    IChannelTagRepository channelTagRepository,
    ITagRepository tagRepository,
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
        var tag =  await tagRepository.GetQuery().FirstOrDefaultAsync(x => x.TagCode == context.Message.TagCode);
        await ChannelRead(tag);
    }

    public async Task Consume(ConsumeContext<TagValueUpdatedEvent> context)
    {
        var tag =  await tagRepository.GetQuery().FirstOrDefaultAsync(x => x.TagCode == context.Message.TagCode);
        await ChannelRead(tag);
    }
    public async Task Consume(ConsumeContext<TagUpdatedEvent> context)
    {
        var tag =  await tagRepository.GetQuery().FirstOrDefaultAsync(x => x.TagCode == context.Message.TagCode);
        await ChannelRead(tag);
    }

    public async Task ChannelRead(TagEntity tag)
    {
        var channels = await channelTagRepository.GetQuery(true).Where(x => x.TagCode == tag.TagCode).ToListAsync();
        if (channels is not null)
        {
            foreach (var channelTagEntity in channels)
            {
                channelTagEntity.Value = tag.Value;
                channelTagEntity.UpdateTime =tag.UpdateTime;
                channelTagEntity.LastValue = tag.LastValue;
                channelTagEntity.LastUpdateTime = tag.LastUpdateTime;
                await channelTagRepository.SaveChangesAsync();
                var channel = await channelRepository.GetQuery(true)
                    .FirstOrDefaultAsync(x => x.ChannelCode == channelTagEntity.ChannelCode);

                //实时发送全部
                if (!channel.IsSchedule )
                {
                    var channelTags = await channelTagRepository.GetQuery()
                        .Where(x => x.ChannelCode == channelTagEntity.ChannelCode).ToListAsync();
                    var tags = new List<ChannelTagDTO>();
                    foreach (var channelTag in channelTags)
                    {
                        var dto = new ChannelTagDTO(
                            channelTag.GroupCode,
                            channelTag.ChannelCode,
                            tag.DriverCode,
                            channelTag.EquipCode,
                            channelTag.TagCode,
                            channelTag.DataType,
                            channelTag.Desc,
                            channelTag.Value == null ? "" : channelTag.Value.GetValue().ToString());

                        tags.Add(dto);
                    }
                    await _publishEndpoint.Publish(
                        new ChannelTagChangedEvent(tags,channel.ChannelCode,channel.SenderCodes));
                }
            }
        }
    }

 
}