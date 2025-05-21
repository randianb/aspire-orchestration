using ContentPlatform.Api.Repository.Sender;
using Contracts;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using InfluxDB.LineProtocol.Client;
using InfluxDB.LineProtocol.Payload;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ContentPlatform.Api.Busi.Sender.EventHandler;

public class InfluxdbSenderConsumer(ILogger<InfluxdbSenderConsumer> logger, ISenderRepository senderRepository)
    : IConsumer<InfluxdbSenderInvokeEvent>
{
    public async Task Consume(ConsumeContext<InfluxdbSenderInvokeEvent> context)
    {
        var sender = await senderRepository.GetQuery()
            .FirstOrDefaultAsync(x => x.SenderCode == context.Message.SenderCode);
        if (null == sender)
        {
            return;
        }

        if (sender.Options == null || !sender.Options.ContainsKey("Url") || !sender.Options.ContainsKey("Db") ||
            !sender.Options.ContainsKey("Measurement"))
        {
            logger.LogInformation("配置不满足条件！");
            return;
        }

        var db = sender.Options.GetValue("Db").ToString();
        var measurement = sender.Options.GetValue("Measurement").ToString();
        var influxDbUrl = sender.Options.GetValue("Url").ToString();

        var tagDtos = context.Message.TagDtos;
        logger.LogInformation($"准备发送 {tagDtos.Count} 条数据到 InfluxDB");

        var timestamp = DateTime.UtcNow; // InfluxDB 的时间戳
        var client = new LineProtocolClient(new Uri(influxDbUrl), db);
        var payload = new LineProtocolPayload();
        var points = tagDtos.Select(x => NewPoint(timestamp, measurement, x)).ToList();
        foreach (var point in points)
        {
            payload.Add(point);
        }

        var result = await client.WriteAsync(payload);
        if (!result.Success)
        {
            logger.LogError("Influxdb encountered an error: {0}", result.ErrorMessage);
        }
    }

    private LineProtocolPoint NewPoint(DateTime timestamp, string measurement, ChannelTagDTO dto)
    {
        var point = PointData.Measurement(measurement);
        Dictionary<string, object> fields = new Dictionary<string, object>();
        Dictionary<string, string> tags = new Dictionary<string, string>();
        // 构建 Line Protocol 字符串
        // 可以根据你的实际需求定义 measurement 
        if (!string.IsNullOrEmpty(dto.GroupCode)) tags.Add("group", dto.GroupCode);
        if (!string.IsNullOrEmpty(dto.ChannelCode)) tags.Add("channel", dto.ChannelCode);
        if (!string.IsNullOrEmpty(dto.DriverCode)) tags.Add("driver", dto.DriverCode);
        if (!string.IsNullOrEmpty(dto.EquipCode)) tags.Add("equip", dto.EquipCode);
        tags.Add("tag", dto.TagCode);
        // 根据 DataType 进行类型转换
        if (dto.DataType.ToLower() == "int" && int.TryParse(dto.Value, out var intValue))
        {
            fields.Add(dto.TagCode, intValue);
        }
        else if (dto.DataType.ToLower() == "double" && double.TryParse(dto.Value, out var doubleValue))
        {
            fields.Add(dto.TagCode, doubleValue);
        }
        else if (dto.DataType.ToLower() == "long" && long.TryParse(dto.Value, out var longValue))
        {
            fields.Add(dto.TagCode, longValue);
        }
        else if (dto.DataType.ToLower() == "uint" && uint.TryParse(dto.Value, out var uintValue))
        {
            fields.Add(dto.TagCode, uintValue);
        }
        else if (dto.DataType.ToLower() == "float" && float.TryParse(dto.Value, out var floatValue))
        {
            fields.Add(dto.TagCode, floatValue);
        }
        else if (dto.DataType.ToLower() == "byte" && byte.TryParse(dto.Value, out var byteValue))
        {
            fields.Add(dto.TagCode, byteValue);
        }
        else if (dto.DataType.ToLower() == "decimal" && Decimal.TryParse(dto.Value, out var DecimalValue))
        {
            fields.Add(dto.TagCode, DecimalValue);
        }
        else if (dto.DataType.ToLower() == "bool" && bool.TryParse(dto.Value, out var boolValue))
        {
            fields.Add(dto.TagCode, boolValue);
        }
        else if (dto.DataType.ToLower() == "ulong" && ulong.TryParse(dto.Value, out var ulongValue))
        {
            fields.Add(dto.TagCode, ulongValue);
        }
        else
        {
            fields.Add(dto.TagCode, dto.Value); // 默认作为字符串处理
        }

        return new LineProtocolPoint(measurement, fields, tags, timestamp);
    }
}