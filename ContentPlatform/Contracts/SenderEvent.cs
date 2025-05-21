using Newtonsoft.Json.Linq;

namespace Contracts;
public record SenderDeletedEvent(Guid Id);
public record SenderCreatedEvent(Guid Id,
   string SenderCode,
   string MachineCode,
   string DriverCode,
   int SenderType,
   JObject? Options,
   string Desc
);

public record SenderUpdatedEvent(Guid Id);
public record DkSenderInvokeEvent(List<ChannelTagDTO> TagDtos,string ChannelCode,string SenderCode);
public record InfluxdbSenderInvokeEvent(List<ChannelTagDTO> TagDtos,string ChannelCode,string SenderCode);
public record Influxdb2SenderInvokeEvent(List<ChannelTagDTO> TagDtos,string ChannelCode,string SenderCode);