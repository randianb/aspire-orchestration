using System.Text;
using ContentPlatform.Api.Entities;
using ContentPlatform.Api.Repository.Channel;
using ContentPlatform.Api.Repository.Sender;
using ContentPlatform.Api.Repository.Tag;
using Contracts;
using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Newtonsoft.Json;
using Tipa.JsonExtern;

namespace ContentPlatform.Api.Busi.Sender.EventHandler;

public class DkSenderConsumer(
    ILogger<DkSenderConsumer> logger,
    IHttpClientFactory httpClientFactory,
    IChannelRepository channelRepository,
    IDistributedCache _distributedCache,
    ISenderRepository senderRepository,
    IRequestResponseLogRepository requestResponseLogRepository,
    IChannelTagHistoryRepository channelTagHistoryRepository) : IConsumer<DkSenderInvokeEvent>
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    private List<string> plcControlList = new List<string>() { "0", "1", "17", "49", "53", "55" };

    public async Task Consume(ConsumeContext<DkSenderInvokeEvent> context)
    {
        if (!await _semaphore.WaitAsync(TimeSpan.FromSeconds(1)))
        {
            logger.LogInformation("无法锁定链接");
        }

        try
        {
            var sender = await senderRepository.GetQuery()
                .FirstOrDefaultAsync(x => x.SenderCode == context.Message.SenderCode);
            if (null == sender)
            {
                return;
            }
            //ControlTag 为 0->1 如果没有条码，丢弃1,然后 0->17 可以触发事件 入站
            //ControlTag 为 49->0 如果没有49，53->0 可以触发事件 出站
            //不接受ControlTag 为 55的数据


            var channel = await channelRepository.GetQuery()
                .FirstOrDefaultAsync(x => x.ChannelCode == context.Message.ChannelCode);
            var httpClient = httpClientFactory.CreateClient();
            if (sender.Options == null || !sender.Options.ContainsKey("Url") || !sender.Options.ContainsKey("ESN") ||
                !sender.Options.ContainsKey("Control"))
            {
                logger.LogInformation("配置不满足条件！");
                return;
            }

            var esnTagCode = sender.Options.GetValue("ESN").ToString();
            var controlTagCode = sender.Options.GetValue("Control").ToString();
            var esnTag = context.Message.TagDtos.FirstOrDefault(x => x.TagCode == esnTagCode);

            //如果条码为空就不记录,把当前的Tag 的Desc 设置 ESN的值
            var (curControlTag, hisControlTag) =
                await getControlHis(context, controlTagCode, esnTag);

            if (curControlTag == null)
            {
                logger.LogInformation("tag  empty not bind control Tag");
                return;
            }

            bool inbound = false;
            bool outbound = false;


            await saveHistory(curControlTag, esnTag);
            if (!plcControlList.Contains(curControlTag.Value))
            {
                logger.LogInformation("未知信号点，不做运算");
                return;
            }

            if ((hisControlTag == null || hisControlTag.Value == null) &&
                (curControlTag.Value == "1" || curControlTag.Value == "17") &&
                esnTag.Value != null)
            {
                //可以入站
                inbound = true;
            }
            else if (hisControlTag != null && hisControlTag.Value != null)
            {
                if (hisControlTag.Value == "0")
                {
                    if ((curControlTag.Value == "1") && (esnTag.Value == null || esnTag.Value.Contains("null")))
                    {
                        return;
                    }
                    else if ((curControlTag.Value == "1" || curControlTag.Value == "17") &&
                             (esnTag.Value != null && !esnTag.Value.Contains("null")))
                    {
                        inbound = true;
                    }
                }
                else if (curControlTag.Value == "0")
                {
                    if ((hisControlTag.Value == "49" || hisControlTag.Value == "53") &&
                        (hisControlTag.Desc != null && !hisControlTag.Desc.Contains("null")))
                    {
                        outbound = true;
                    }
                }
                else if (curControlTag.Value == "55")
                {
                    return;
                }
            }

            httpClient.BaseAddress = new Uri(sender.Options.GetValue("Url").ToString());
            logger.LogInformation(
                $"DKESNConsumer consumed message: \r\n {JsonConvert.SerializeObject(context.Message.TagDtos)}");

            await saveMemo(context, controlTagCode, curControlTag, esnTag, hisControlTag);

            if (!inbound && !outbound)
            {
                return;
            }

            try
            {
                string requestUri = "";
                if (inbound) requestUri = "/adne/scanCage/inbound";
                if (outbound) requestUri = "/adne/scanCage/outbound";
                HttpResponseMessage response = null;
                var requestBody = new PostRequest(esnTag.Value, curControlTag.GroupCode);
                string requestMethod = HttpMethod.Post.Method;
                string responseBody = null;
                try
                {
                    if (inbound)
                    {
                        requestBody = new PostRequest(esnTag.Value, curControlTag.GroupCode);
                        response = await httpClient.PostAsJsonAsync(requestUri,
                            requestBody);
                    }
                    else if (outbound)
                    {
                        requestBody = new PostRequest(hisControlTag.Desc, curControlTag.GroupCode);
                        response = await httpClient.PostAsJsonAsync(requestUri,
                            requestBody);
                    }


                    if (response != null)
                    {
                        response.EnsureSuccessStatusCode(); // 拋出異常如果狀態碼不是成功的範圍
                        responseBody = await response.Content.ReadAsStringAsync();
                    }

                    await requestResponseLogRepository.CreateAsync(new RequestResponseLogEntity
                    {
                        Timestamp = DateTime.UtcNow,
                        RequestUri = requestUri,
                        RequestMethod = requestMethod,
                        RequestBody = JsonConvert.SerializeObject(requestBody),
                        ResponseStatusCode = (int?)response?.StatusCode,
                        ResponseBody = responseBody,
                        IsError = false,
                        ErrorMessage = null
                    });
                    await requestResponseLogRepository.SaveChangesAsync();
                }
                catch (HttpRequestException ex)
                {
                    await requestResponseLogRepository.CreateAsync(new RequestResponseLogEntity
                    {
                        Timestamp = DateTime.UtcNow,
                        RequestUri = requestUri,
                        RequestMethod = requestMethod,
                        RequestBody = JsonConvert.SerializeObject(requestBody),
                        ResponseStatusCode = (int?)response?.StatusCode,
                        ResponseBody = responseBody,
                        IsError = true,
                        ErrorMessage = ex.Message
                    });
                    await requestResponseLogRepository.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "DKESNConsumer error");
                    await requestResponseLogRepository.CreateAsync(new RequestResponseLogEntity
                    {
                        Timestamp = DateTime.UtcNow,
                        RequestUri = requestUri,
                        RequestMethod = requestMethod,
                        RequestBody = JsonConvert.SerializeObject(requestBody),
                        ResponseStatusCode = (int?)response?.StatusCode,
                        ResponseBody = responseBody,
                        IsError = true,
                        ErrorMessage = ex.Message
                    });
                    await requestResponseLogRepository.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task saveMemo(ConsumeContext<DkSenderInvokeEvent> context, string controlTagCode,
        ChannelTagDTO curControlTag,
        ChannelTagDTO esnTag, ChannelTagDTO hisTag)
    {
        // 确保 key 是唯一的且稳定
        var cacheKey = context.Message.ChannelCode + "-dk-" +
                       controlTagCode; // IDistributedCache 使用 byte[] 作为 key
        // 序列化当前值
        try
        {
            // 3. **记录/存储** 当前的值供下次使用 (使用 IDistributedCache)
            if (curControlTag != null)
            {
                //如果是空条码
                if (curControlTag.Value != "0" && (null == esnTag.Value || esnTag.Value.Contains("null")))
                {
                    return;
                }
                else if (curControlTag.Value == "55")
                {
                    //不缓存55
                    return;
                }
                else if (curControlTag.Value == "53" && (null == esnTag.Value || esnTag.Value.Contains("null")))
                {
                    //不缓存53 条码为空的
                    return;
                }
            }

            curControlTag = curControlTag with
            {
                Desc = ((null == esnTag.Value || esnTag.Value.Contains("null") ? hisTag.Desc : esnTag.Value))
            };
            string currentJson = JsonConvert.SerializeObject(curControlTag);
            byte[] currentBytes = Encoding.UTF8.GetBytes(currentJson);

            // 设置缓存选项（过期时间等）
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(10) // 例如，缓存 10 分钟
            };

            // 存储到分布式缓存
            await _distributedCache.SetAsync(cacheKey, currentBytes, cacheOptions, context.CancellationToken);
            Console.WriteLine($"当前 {controlTagCode} 的值已记录到分布式缓存。");
        }
        catch (JsonException ex)
        {
            Console.WriteLine(
                $"Error serializing current data for key {cacheKey}: {ex.Message}");
            // 处理序列化错误，可能无法缓存当前值
        }
    }

    private async Task saveHistory(ChannelTagDTO curControlTag, ChannelTagDTO? esnTag)
    {
        try
        {
            var chanelTags = new List<ChannelTagEntity>();


            var curChannelTag = curControlTag.Adapt<ChannelTagEntity>();
            var esnChannelTag = esnTag.Adapt<ChannelTagEntity>();
            curChannelTag.Value = new ObjValue() { Str = curControlTag.Value };
            esnChannelTag.Value = new ObjValue() { Str = esnTag.Value };

            chanelTags.Add(curChannelTag);
            chanelTags.Add(esnChannelTag);
            await channelTagHistoryRepository.CreateAsync(new ChannelTagHistoryEntity()
            {
                ChannelCode = curControlTag.ChannelCode,
                Body = chanelTags,
                SimpleBody = new Dictionary<string, string>()
                {
                    { curControlTag.TagCode, curControlTag.Value },
                    { esnTag.TagCode, esnTag.Value },
                },
                CreateTime = DateTime.UtcNow
            });
            await channelTagHistoryRepository.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
        }
    }

    private async Task<(ChannelTagDTO? curControlTag, ChannelTagDTO? hisControlTag)> getControlHis(
        ConsumeContext<DkSenderInvokeEvent> context, string controlTagCode, ChannelTagDTO esnTag)
    {
        //记录上一次值

        var curControlTag = context.Message.TagDtos.FirstOrDefault(x => x.TagCode == controlTagCode);

        // 确保 key 是唯一的且稳定
        var cacheKey = context.Message.ChannelCode + "-dk-" +
                       controlTagCode; // IDistributedCache 使用 byte[] 作为 key

        ChannelTagDTO hisControlTag = null;

        // 1. **获取** 上次记录的值 (使用 IDistributedCache)
        //    IDistributedCache.GetAsync 返回 byte[] 或 null
        byte[] cachedBytes = await _distributedCache.GetAsync(cacheKey, context.CancellationToken);

        if (cachedBytes != null && cachedBytes.Length > 0)
        {
            // 反序列化缓存的字节数组为 ChannelTagDTO
            // 你需要根据你的序列化方式来反序列化，这里以 Newtonsoft.Json 为例
            try
            {
                string cachedJson = Encoding.UTF8.GetString(cachedBytes);
                hisControlTag = JsonConvert.DeserializeObject<ChannelTagDTO>(cachedJson);
            }
            catch (JsonException ex)
            {
                // 处理反序列化错误，可能缓存数据已损坏或格式改变
                Console.WriteLine(
                    $"Error deserializing historical data for key {cacheKey}: {ex.Message}");
                // 可以选择移除损坏的缓存项
                await _distributedCache.RemoveAsync(cacheKey, context.CancellationToken);
                hisControlTag = null; // 重置为 null，表示无法获取上次有效值
            }
        }


        // ... 后续处理 ...
        return (curControlTag, hisControlTag);
    }
}

public record PostRequest(
    string esn,
    string workStation);