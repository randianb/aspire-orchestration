namespace Contracts;

public record TagCreatedEvent(Guid Id,
string? GroupCode,
string? DriverCode,
string? EquipCode,
string TagCode,
string DataType,
string? Desc,
string? Value
);

public record TagValueUpdatedEvent(
    string TagCode,
    string? LastUpdateValue,
    DateTime? LastUpdateTime,
    string? Value,
    DateTime? UpdateTime
);
public record TagUpdatedEvent(Guid Id,
    string? GroupCode,
    string? DriverCode,
    string? EquipCode,
    string TagCode,
    string DataType,
    string? Desc,
    string? Value
);
public record TagDeletedEvent(Guid Id);
public record TagReadedEvent(Guid Id,DateTime ReadedAt);
