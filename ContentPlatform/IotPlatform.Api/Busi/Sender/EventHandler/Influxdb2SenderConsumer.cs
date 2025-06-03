using Contracts;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using IotPlatform.Api.Repository.Sender;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace IotPlatform.Api.Busi.Sender.EventHandler;

public class Influxdb2SenderConsumer(ILogger<Influxdb2SenderConsumer> logger, ISenderRepository senderRepository)
    : IConsumer<Influxdb2SenderInvokeEvent>
{
    public async Task Consume(ConsumeContext<Influxdb2SenderInvokeEvent> context)
    {
        var sender = await senderRepository.GetQuery()
            .FirstOrDefaultAsync(x => x.SenderCode == context.Message.SenderCode);
        if (null == sender)
        {
            return;
        }

//jIxuUBrCuEf_YMzAku0L84sX0iwNh-0s05qUtPbCr7dS9raF5K13oldsepxY4CmwWg0kHQeIWOpjta6R8-oQ7w==
        if (sender.Options == null || !sender.Options.ContainsKey("Url") || !sender.Options.ContainsKey("Measurement")
            || !sender.Options.ContainsKey("Token")
            // || !sender.Options.ContainsKey("Bucket")
           )
        {
            logger.LogInformation("配置不满足条件！");
            return;
        }

        var measurement = sender.Options.GetValue("Measurement").ToString();
        var influxDbUrl = sender.Options.GetValue("Url").ToString();
        var token = sender.Options.GetValue("Token").ToString();
        var _org = sender.Options.GetValue("Org").ToString();
        var _bucket = sender.Options.GetValue("Bucket").ToString();
        var tagDtos = context.Message.TagDtos;
        logger.LogInformation($"准备发送 {tagDtos.Count} 条数据到 InfluxDB");

        var timestamp = DateTime.UtcNow; // InfluxDB 的时间戳
        var options = new InfluxDBClientOptions.Builder()
            .AuthenticateToken(token)
            .Org(_org)
            .Bucket(_bucket)
            .Url(influxDbUrl)
            .Build();

        var influxDbClient = new InfluxDBClient(options);
        var writeApi = influxDbClient.GetWriteApiAsync();
        var points = tagDtos.Select(x => NewPoint(timestamp, measurement,x)).ToList();
        await writeApi.WritePointsAsync(points);
    }

    private PointData NewPoint(DateTime timestamp, string measurement,ChannelTagDTO dto)
    {
        var point = PointData.Measurement(measurement);
        point.Timestamp(timestamp, WritePrecision.Ns);
        // 构建 Line Protocol 字符串
        // 可以根据你的实际需求定义 measurement 
        if (!string.IsNullOrEmpty(dto.GroupCode)) point=point.Tag("group", dto.GroupCode);
        if (!string.IsNullOrEmpty(dto.ChannelCode)) point=point.Tag("channel", dto.ChannelCode);
        if (!string.IsNullOrEmpty(dto.DriverCode)) point=point.Tag("driver", dto.DriverCode);
        if (!string.IsNullOrEmpty(dto.EquipCode)) point=point.Tag("equip", dto.EquipCode);
        point=point.Tag("tag", dto.TagCode);
        // 根据 DataType 进行类型转换
        if (dto.DataType.ToLower() == "int" && int.TryParse(dto.Value, out var intValue))
        {
            point=point.Field(dto.TagCode, intValue);
        }
        else if (dto.DataType.ToLower() == "double" && double.TryParse(dto.Value, out var doubleValue))
        {
            point=point.Field(dto.TagCode, doubleValue);
        }
        else if (dto.DataType.ToLower() == "long" && long.TryParse(dto.Value, out var longValue))
        {
            point=point.Field(dto.TagCode, longValue);
        }
        else if (dto.DataType.ToLower() == "uint" && uint.TryParse(dto.Value, out var uintValue))
        {
            point=point.Field(dto.TagCode, uintValue);
        }
        else if (dto.DataType.ToLower() == "float" && float.TryParse(dto.Value, out var floatValue))
        {
            point=point.Field(dto.TagCode, floatValue);
        }
        else if (dto.DataType.ToLower() == "byte" && byte.TryParse(dto.Value, out var byteValue))
        {
            point=point.Field(dto.TagCode, byteValue);
        }
        else if (dto.DataType.ToLower() == "decimal" && Decimal.TryParse(dto.Value, out var DecimalValue))
        {
            point=point.Field(dto.TagCode, DecimalValue);
        }
        else if (dto.DataType.ToLower() == "bool" && bool.TryParse(dto.Value, out var boolValue))
        {
            point=point.Field(dto.TagCode, boolValue);
        }
        else if (dto.DataType.ToLower() == "ulong" && ulong.TryParse(dto.Value, out var ulongValue))
        {
            point=point.Field(dto.TagCode, ulongValue);
        }
        else
        {
            point=point.Field(dto.TagCode, dto.Value); // 默认作为字符串处理
        }
        return point;
    }
}