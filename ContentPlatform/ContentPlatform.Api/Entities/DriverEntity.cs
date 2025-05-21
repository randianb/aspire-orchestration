using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;

namespace ContentPlatform.Api.Entities;

public class DriverEntity
{
    [Key] public Guid Id { get; set; }

    public string DriverCode { get; set; }
    public int DriverType { get; set; }
    public string? MachineCode { get; set; }
    public string? ServerName { get; set; }
    public string ServerUrl { get; set; }
    public bool HasIdentity { get; set; }
    public string? UserName { get; set; }
    public string? PassWord { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime? UpdateTime { get; set; }
}

public class EquipEntity
{
    [Key] public Guid Id { get; set; }

    public string EquipCode { get; set; }
    public string EquipName { get; set; }
    public string? Desc { get; set; }
    public DateTime CreateTime { get; set; }
}

public class GroupEntity
{
    [Key] public Guid Id { get; set; }

    public string EquipCode { get; set; }
    public string GroupCode { get; set; }
    public string? Desc { get; set; }
    public DateTime CreateTime { get; set; }
}

public class TagEntity
{
    [Key] public Guid Id { get; set; }

    public string? GroupCode { get; set; }
    public string? DriverCode { get; set; }
    public string? EquipCode { get; set; }
    public string TagCode { get; set; }
    public string DataType { get; set; }
    public string? Desc { get; set; }
    public double? Scaling { get; set; }
    public double? Shifting { get; set; }

    [Column(TypeName = "jsonb")] // PostgreSQL specific, use "json" for MySQL/SQL Server
    public string? ValueJson { get; set; }

    public DateTime? UpdateTime { get; set; }
    public DateTime CreateTime { get; set; }

    [Column(TypeName = "jsonb")] // PostgreSQL specific, use "json" for MySQL/SQL Server
    public string? LastValueJson { get; set; }

    public DateTime? LastUpdateTime { get; set; }

    [JsonIgnore] // Prevent EF Core from trying to map this property
    public ObjValue? Value
    {
        get
        {
            if (string.IsNullOrEmpty(ValueJson))
            {
                return null;
            }

            return JsonSerializer.Deserialize<ObjValue>(ValueJson);
        }
        set
        {
            if (value == null)
            {
                ValueJson = null;
            }
            else
            {
                ValueJson = JsonSerializer.Serialize(value);
            }
        }
    }

    [JsonIgnore] // Prevent EF Core from trying to map this property
    public ObjValue? LastValue
    {
        get
        {
            if (string.IsNullOrEmpty(LastValueJson))
            {
                return null;
            }

            return JsonSerializer.Deserialize<ObjValue>(LastValueJson);
        }
        set
        {
            if (value == null)
            {
                LastValueJson = null;
            }
            else
            {
                LastValueJson = JsonSerializer.Serialize(value);
            }
        }
    }
}

public class SchedulerConfigEntity
{
    [Key] public Guid Id { get; set; }

    public string Topic { get; set; }
    public string Body { get; set; }
    public int Round { get; set; }
    public string Expression { get; set; }
    public DateTime? UpdateTime { get; set; }
    public DateTime CreateTime { get; set; }
}

public class ChannelEntity
{
    [Key] public Guid Id { get; set; }

    public string ChannelCode { get; set; }
    public bool IsSchedule { get; set; }
    public bool? IsFull { get; set; }
    public string Topic { get; set; }
    public string Desc { get; set; }
    public List<string> SenderCodes { get; set; } = new();
    public List<string> TagCodes { get; set; } = new();
}

public class MachineEntity
{
    [Key] public Guid Id { get; set; }

    public string MachineCode { get; set; }
    public string? Desc { get; set; }
    public DateTime? UpdateTime { get; set; }
    public DateTime CreateTime { get; set; }
}

public class ChannelTagHistoryEntity
{
    [Key] public Guid Id { get; set; }

    public string ChannelCode { get; set; }

    [Column(TypeName = "jsonb")] // PostgreSQL specific, use "json" for MySQL/SQL Server
    public string? BodyJson { get; set; }

    public List<ChannelTagEntity> Body 
    {
        get
        {
            if (string.IsNullOrEmpty(BodyJson))
            {
                return null;
            }

            return JsonSerializer.Deserialize<List<ChannelTagEntity>>(BodyJson);
        }
        set
        {
            if (value == null)
            {
                BodyJson = null;
            }
            else
            {
                BodyJson = JsonSerializer.Serialize(value);
            }
        }
    }

    [Column(TypeName = "jsonb")] // PostgreSQL specific, use "json" for MySQL/SQL Server
    public string? SimpleBodyJson { get; set; }

    public Dictionary<string, string> SimpleBody
    {
        get
        {
            if (string.IsNullOrEmpty(SimpleBodyJson))
            {
                return null;
            }

            return JsonSerializer.Deserialize<Dictionary<string, string>>(SimpleBodyJson);
        }
        set
        {
            if (value == null)
            {
                SimpleBodyJson = null;
            }
            else
            {
                SimpleBodyJson = JsonSerializer.Serialize(value);
            }
        }
    }

    public DateTime CreateTime { get; set; }
}

public class ChannelTagEntity
{
    [Key] public Guid Id { get; set; }

    public string ChannelCode { get; set; }

    public string? GroupCode { get; set; }
    public string? EquipCode { get; set; }
    public string TagCode { get; set; }
    public string? DriverCode { get; set; }
    public string DataType { get; set; }
    public string? Desc { get; set; }
    public DateTime? LastUpdateTime { get; set; }
    public DateTime? UpdateTime { get; set; }
    public DateTime CreateTime { get; set; }

    [Column(TypeName = "jsonb")] // PostgreSQL specific, use "json" for MySQL/SQL Server
    public string? ValueJson { get; set; }

    [Column(TypeName = "jsonb")] // PostgreSQL specific, use "json" for MySQL/SQL Server
    public string? LastValueJson { get; set; }

    [JsonIgnore] // Prevent EF Core from trying to map this property
    public ObjValue? Value
    {
        get
        {
            if (string.IsNullOrEmpty(ValueJson))
            {
                return null;
            }

            return JsonSerializer.Deserialize<ObjValue>(ValueJson);
        }
        set
        {
            if (value == null)
            {
                ValueJson = null;
            }
            else
            {
                ValueJson = JsonSerializer.Serialize(value);
            }
        }
    }

    [JsonIgnore] // Prevent EF Core from trying to map this property
    public ObjValue? LastValue
    {
        get
        {
            if (string.IsNullOrEmpty(LastValueJson))
            {
                return null;
            }

            return JsonSerializer.Deserialize<ObjValue>(LastValueJson);
        }
        set
        {
            if (value == null)
            {
                LastValueJson = null;
            }
            else
            {
                LastValueJson = JsonSerializer.Serialize(value);
            }
        }
    }
}
public class RequestResponseLogEntity
{
    public Guid Id { get; set; }
    public DateTime? Timestamp { get; set; }
    public string RequestUri { get; set; }
    public string RequestMethod { get; set; }
    public string RequestBody { get; set; }
    public int? ResponseStatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public bool? IsError { get; set; }
    public string? ErrorMessage { get; set; }
    // 可以根據需要添加更多欄位，例如請求頭、響應頭等
}
public class SenderEntity
{
    [Key] public Guid Id { get; set; }

    public string SenderCode { get; set; }
    public string? MachineCode { get; set; }
    public string? DriverCode { get; set; }
    public int SenderType { get; set; }
    [Column(TypeName = "jsonb")] // PostgreSQL specific, use "json" for MySQL/SQL Server
    public string? OptionsJson { get; set; }
    public JObject? Options  {
        get
        {
            if (string.IsNullOrEmpty(OptionsJson))
            {
                return null;
            }

            return JObject.Parse(OptionsJson);
        }
        set
        {
            if (value == null)
            {
                OptionsJson = null;
            }
            else
            {
                OptionsJson = JsonSerializer.Serialize(value);
            }
        }
    }
    public string? Desc { get; set; }
}

public record ObjValue
{
    [JsonPropertyName("str")] public string? Str { get; init; }
    [JsonPropertyName("uint32")] public uint? Uint32 { get; init; }
    [JsonPropertyName("boolean")] public bool? Boolean { get; init; }
    [JsonPropertyName("int16")] public short? Int16 { get; init; }
    [JsonPropertyName("uint16")] public ushort? Uint16 { get; init; }
    [JsonPropertyName("int32")] public int? Int32 { get; init; }
    [JsonPropertyName("long")] public long? Long { get; init; }
    [JsonPropertyName("ulong")] public ulong? Ulong { get; init; }
    [JsonPropertyName("float")] public float? Float { get; init; }
    [JsonPropertyName("double")] public double? Double { get; init; }
    [JsonPropertyName("decimal")] public decimal? Decimal { get; init; }
    [JsonPropertyName("byte")] public byte? Byte { get; init; }

    public object? GetValue()
    {
        if (Str != null) return Str;
        if (Uint32 != null) return Uint32;
        if (Boolean != null) return Boolean;
        if (Int16 != null) return Int16;
        if (Uint16 != null) return Uint16;
        if (Int32 != null) return Int32;
        if (Long != null) return Long;
        if (Ulong != null) return Ulong;
        if (Float != null) return Float;
        if (Double != null) return Double;
        if (Decimal != null) return Decimal;
        if (Byte != null) return Byte;
        return null;
    }
}