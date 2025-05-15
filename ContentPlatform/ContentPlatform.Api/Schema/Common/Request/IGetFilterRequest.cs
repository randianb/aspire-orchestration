namespace ContentPlatform.Api.Schema.Common.Request;

public interface IGetFilterRequest<TSource>
{
    TSource Filter { get; set; }
    public string? FuncFilter { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
    public List<string>? Sort { get; set; }
}