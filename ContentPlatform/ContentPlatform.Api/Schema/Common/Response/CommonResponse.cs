namespace ContentPlatform.Api.Schema.Common.Response;

public abstract class CommonResponse
{
    //创建时间
    public DateTime CreatedAt { get; set; }

    //更新时间
    public DateTime UpdatedAt { get; set; }
}