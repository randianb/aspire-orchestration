namespace ContentPlatform.Api.Schema.Common.Response;

public class ListEntity<TEntiry>
{
    public int Total { get; set; }
    public List<TEntiry> Items { get; set; }
}