using ContentPlatform.Api.Entities;
using Contracts;

namespace ContentPlatform.Api.DataMap;

public static class ChannelTagMap
{
    public static List<ChannelTagDTO> ChannelTagDtos( List<ChannelTagEntity> channelTags)
    {
        var tags = new List<ChannelTagDTO>();
        foreach (var channelTag in channelTags)
        {
            var dto = new ChannelTagDTO(
                channelTag.GroupCode,
                channelTag.ChannelCode,
                channelTag.DriverCode,
                channelTag.EquipCode,
                channelTag.TagCode,
                channelTag.DataType,
                channelTag.Desc,
                channelTag.Value == null ? "" : channelTag.Value.GetValue().ToString());

            tags.Add(dto);
        }

        return tags;
    }
}