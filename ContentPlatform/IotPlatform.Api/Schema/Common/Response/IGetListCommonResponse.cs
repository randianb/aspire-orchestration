namespace IotPlatform.Api.Schema.Common.Response;

public interface IGetListCommonResponse<T>
{
    List<T> Items { get; set; }
    int Total { get; set; }
}