namespace IotPlatform.Api.Schema.Common.Request;

public class GetCommonRequest
{
    //请求最大条数
    public int Limit { get; set; } = 10;
    //请求偏移量
    public int Offset { get; set; } = 0;
    //排序字段，升序asc，降序desc，如CreatedAt_desc
    public List<string>? Sort { get; set; } = null;
}