using System.Data.Common;
using IotPlatform.Api.Repository;
using IotPlatform.Api.Busi.Logic.Common;
using IotPlatform.Api.Busi.Tag.Api;
using IotPlatform.Api.Entities;
using IotPlatform.Api.Repository.Tag;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IotPlatform.Api.Busi.Logic;

public class TagService(ITagRepository iTagRepository,ISender sender, ValueParserService valueParserService)
{

    public async Task AddOrUpdateTagAsync(string se, string value)
    {
        try
        {
            var tag = await iTagRepository.GetQuery(true).FirstOrDefaultAsync(x=>x.TagCode==se);
            if (tag != null)
            {
                valueParserService.ConvertType(value, tag);
                var command = tag.Adapt<UpdateTagValue.Command>();
                var result = await sender.Send(command);
            }
            else
            {
                var tagEntity = new TagEntity()
                {
                    Id = Guid.NewGuid(),
                    TagCode= se,
                    DataType= "string",
                    CreateTime= DateTime.UtcNow,
                    Value = new ObjValue(){Str = value}
                };
                var command = tagEntity.Adapt<CreateTag.Command>();
                var result = await sender.Send(command);
            }

         
        }
        catch (Exception ex)
        {
            // 处理异常
            throw;
        }
        finally
        {
        }
    }
}
