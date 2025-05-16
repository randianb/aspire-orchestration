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
    IConsumer<TagUpdatedEvent>
{
    public async Task Consume(ConsumeContext<TagCreatedEvent> context)
    {
        TagEntity tag = context.Message.Adapt<TagEntity>();
        await ChannelRead(tag);
    }

    public async Task Consume(ConsumeContext<TagUpdatedEvent> context)
    {
        TagEntity tag = new TagEntity();
        tag.Id = context.Message.Id;
        tag.TagCode = context.Message.TagCode;
        tag.DriverCode = context.Message.DriverCode;
        tag.DataType = context.Message.DataType;
        tag.Value = new ObjValue() { Str = context.Message.Value };
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
                channelTagEntity.LastValue = tag.LastValue;
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