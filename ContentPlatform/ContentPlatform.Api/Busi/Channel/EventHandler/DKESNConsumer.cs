using ContentPlatform.Api.Repository.Channel;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.VisualBasic;
using Newtonsoft.Json;

namespace ContentPlatform.Api.Busi.Channel.EventHandler;

public class DKESNConsumer(
    ILogger<DKESNConsumer> logger,
    IHttpClientFactory httpClientFactory,
    HybridCache hybridCache,
    IChannelRepository channelRepository) : IConsumer<DKChannelTagChangedEvent>
{
    public async Task Consume(ConsumeContext<DKChannelTagChangedEvent> context)
    {
        var first = context.Message.TagDtos.FirstOrDefault();
        var esnTag = context.Message.TagDtos.FirstOrDefault(x => x.Desc == "ESN");
        if (esnTag == null)
        {
            logger.LogInformation("No ESN tag found,Desc named ESN");
        }

        var channel = await channelRepository.GetQuery().FirstOrDefaultAsync(x => x.ChannelCode == first.ChannelCode);
        var httpClient = httpClientFactory.CreateClient("Dk");
        logger.LogInformation(
            $"DKESNConsumer consumed message: \r\n {JsonConvert.SerializeObject(context.Message.TagDtos)}");
        var tag = await hybridCache.GetOrCreateAsync(first.ChannelCode + "-dk",
            async (ct) =>
            {
                ChannelTagDTO dto = context.Message.TagDtos.FirstOrDefault(x => x.TagCode == channel.Topic);
                return dto;
            }, cancellationToken: context.CancellationToken);
        var curTag = context.Message.TagDtos.FirstOrDefault(x => x.TagCode == channel.Topic);
        if (curTag == null || tag == null) 
        {
            logger.LogInformation("tag Topic empty not bind control Tag");
            return;
        }
        var cur = curTag.Value == null ? -1 : int.Parse(curTag.Value);
        var history = tag.Value == null ? -1 : int.Parse(tag.Value);
        //如果没有变化就什么都不做
        if (cur == history)
        {
            return;
        }
        else if (history == 0 && cur == 1)
        {
            //入站
            await httpClient.PostAsJsonAsync("/scanCage/inbound", new PostRequest(esnTag.Value, curTag.GroupCode));
        }
        else if (history == 49 && cur == 0)
        {
            //出站
            await httpClient.PostAsJsonAsync("/scanCage/outbound", new PostRequest(esnTag.Value, curTag.GroupCode));
        }
        logger.LogInformation($"DKESNConsumer consumed message\r\n cur:{cur} history：{history} esn:{esnTag.Value} ");
    }
}

public record PostRequest(
    string esn,
    string workStation);