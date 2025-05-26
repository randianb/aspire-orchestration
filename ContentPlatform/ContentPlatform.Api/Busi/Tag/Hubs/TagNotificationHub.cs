using ContentPlatform.Api.Entities;
using Microsoft.AspNetCore.SignalR;

namespace ContentPlatform.Api.Busi.Tag.Hubs;

public interface ITagNotificationClient
{
    Task SendTagValueUpdate(TagEntity tagEntity);
}
public sealed class TagNotificationHub : Hub<ITagNotificationClient>
{
  
}