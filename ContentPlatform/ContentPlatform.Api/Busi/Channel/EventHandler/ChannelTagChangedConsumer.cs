using ContentPlatform.Api.Busi.Sender.Enum;
using ContentPlatform.Api.DataMap;
using ContentPlatform.Api.Repository.Channel;
using ContentPlatform.Api.Repository.Sender;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ContentPlatform.Api.Busi.Channel.EventHandler;

public class ChannelTagChangedConsumer(IPublishEndpoint _publishEndpoint, ISenderRepository senderRepository ,IChannelTagRepository channelTagRepository ,IChannelRepository channelRepository)
    : IConsumer<ChannelTagChangedEvent>
{
    public async Task Consume(ConsumeContext<ChannelTagChangedEvent> context)
    {
        
        var senders = await senderRepository.GetQuery().Where(x => context.Message.SenderCodes.Contains(x.SenderCode))
            .ToListAsync();
        var senderMap = senders.ToDictionary(x => x.SenderCode, x => x);
        var channel = await channelRepository.GetQuery().FirstOrDefaultAsync(x => x.ChannelCode == context.Message.ChannelCode);
        var tags = new List<ChannelTagDTO>();
        if (channel.IsFull is true)
        {
            var channelTags = await channelTagRepository.GetQuery()
                .Where(x => x.ChannelCode == context.Message.ChannelCode )
                .ToListAsync();
            tags = ChannelTagMap.ChannelTagDtos(channelTags);
        }
        else
        {
            tags = context.Message.TagDtos;
        }



        foreach (var senderCode in context.Message.SenderCodes)
        {
            //获取对应的Channel
            //获取对应的Sender
            //如果是即使的
            //如果是调度的
            if (senderMap[senderCode].SenderType == (int)SenderTypeEnum.Dk)
            {
                await _publishEndpoint.Publish(
                    new DkSenderInvokeEvent(tags, context.Message.ChannelCode, senderCode),
                    context.CancellationToken);
            }
            else if (senderMap[senderCode].SenderType == (int)SenderTypeEnum.InfluxDB)
            {
                await _publishEndpoint.Publish(
                    new InfluxdbSenderInvokeEvent(tags, context.Message.ChannelCode, senderCode),
                    context.CancellationToken);
            } else if (senderMap[senderCode].SenderType == (int)SenderTypeEnum.InfluxDB2)
            {
                await _publishEndpoint.Publish(
                    new Influxdb2SenderInvokeEvent(tags, context.Message.ChannelCode, senderCode),
                    context.CancellationToken);
            }
        }
    }
}