namespace Contracts;

public record DriverCreatedEvent(
    Guid Id,
    string DriverCode,
    int DriverType,
    string MachineCode,
    string ServerName,
    string ServerUrl,
    bool HasIdentity,
    string? UserName,
    string? PassWord
);

public record DriverUpdatedEvent(Guid Id);

public record DriverDeletedEvent(Guid Id);
public record DriverRestartedEvent(Guid Id,DateTime RestartedAt);