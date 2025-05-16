using Contracts;
using MassTransit;

namespace ContentPlatform.Api.Busi.Channel.EventHandler;

public class ChannelTagChangedConsumer(IPublishEndpoint _publishEndpoint) : IConsumer<ChannelTagChangedEvent>
{
    public async Task Consume(ConsumeContext<ChannelTagChangedEvent> context)
    {
        foreach (var senderCode in context.Message.SenderCodes)
        {
            //获取对应的Channel
            //获取对应的Sender
            //如果是即使的
            //如果是调度的
            if (senderCode == "Dk")
            {
                await _publishEndpoint.Publish(
                new DkSenderInvokeEvent(context.Message.TagDtos, context.Message.ChannelCode, senderCode),context.CancellationToken);
            }
        }
    }
}