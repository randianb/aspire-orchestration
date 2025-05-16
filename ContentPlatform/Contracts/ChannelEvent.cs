namespace Contracts;

public record ChannelCreatedEvent(
    Guid Id,
    string ChannelCode,
    bool IsSchedule,
    string Topic,
    string Desc,
    List<string> SenderCodes,
    List<string> TagCodes
);

public record ChannelUpdatedEvent(Guid Id);

public record ChannelDeletedEvent(Guid Id);

public record ChannelTagDTO(
    string? GroupCode,
    string? ChannelCode,
    string? DriverCode,
    string? EquipCode,
    string TagCode,
    string DataType,
    string? Desc,
    string? Value
);

public record ChannelTagChangedEvent(List<ChannelTagDTO> TagDtos,String ChannelCode,List<String> SenderCodes);