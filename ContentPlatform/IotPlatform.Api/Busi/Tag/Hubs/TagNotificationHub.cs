using IotPlatform.Api.Entities;
using Microsoft.AspNetCore.SignalR;

namespace IotPlatform.Api.Busi.Tag.Hubs;

public interface ITagNotificationClient
{
    Task SendTagValueUpdate(TagEntity tagEntity);
}
public sealed class TagNotificationHub : Hub<ITagNotificationClient>
{
  
}