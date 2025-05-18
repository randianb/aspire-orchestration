using Carter;
using ContentPlatform.Api.Busi.Logic;
using ContentPlatform.Api.Busi.Logic.EdgeDriver;
using ContentPlatform.Api.Database;
using ContentPlatform.Api.Entities;
using ContentPlatform.Api.Repository;
using ContentPlatform.Api.Repository.Driver;
using ContentPlatform.Api.Repository.Tag;
using Contracts;
using FluentValidation;
using Mapster;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Newtonsoft.Json;
using Shared;

namespace ContentPlatform.Api.Busi.Tag.Api;

public static class UpdateTagValue
{
    public class Request
    {
        public Guid Id { get; set; }
        public string TagCode { get; set; }
        public ObjValue? Value { get; set; }
    }

    public class Request1
    {
        public Guid Id { get; set; }
        public string TagCode { get; set; }
        public string Value { get; set; }
    }

    public class Command : IRequest<Result<Guid>>
    {
        public Guid Id { get; set; }
        public string TagCode { get; set; }
        public ObjValue? Value { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.TagCode).NotEmpty();
        }
    }

    internal sealed class Handler : IRequestHandler<Command, Result<Guid>>
    {
        private readonly ITagRepository _iTagRepository;
        private readonly IValidator<Command> _validator;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly HybridCache _hybridCache;

        public Handler(ITagRepository iTagRepository, IValidator<Command> validator, IPublishEndpoint publishEndpoint,
            HybridCache hybridCache)
        {
            _iTagRepository = iTagRepository;
            _validator = validator;
            _publishEndpoint = publishEndpoint;
            _hybridCache = hybridCache;
        }

        public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                return Result.Failure<Guid>(new Error(
                    "UpdateTagValue.Validation",
                    validationResult.ToString()));
            }

            var tag = await _iTagRepository.GetQuery(true).FirstOrDefaultAsync(x => x.TagCode == request.TagCode,
                cancellationToken: cancellationToken);
            if (tag == null)
            {
                return Result.Failure<Guid>(new Error(
                    "UpdateTagValue.Validation",
                    "not found"));
            }

            tag.LastValueJson = JsonConvert.SerializeObject(tag.Value);
            tag.LastUpdateTime = tag.UpdateTime;
            tag.Value = request.Value;
            tag.UpdateTime = DateTime.UtcNow;

            await _iTagRepository.SaveChangesAsync();

            await _publishEndpoint.Publish(
                new TagValueUpdatedEvent(
                    tag.TagCode,
                    tag.LastValue is null
                        ? null
                        : (tag.LastValue.GetValue() is null ? null : tag.LastValue.GetValue().ToString()),
                    tag.LastUpdateTime,
                    tag.Value is null ? null : (tag.Value.GetValue() is null ? null : tag.Value.GetValue().ToString()),
                    tag.UpdateTime),
                cancellationToken);
            await _hybridCache.RemoveAsync("tags", cancellationToken);
            return tag.Id;
        }
    }
}

public class UpdateTagValueEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("api/tag/updateValue/", async (UpdateTagValue.Request request, ISender sender) =>
        {
            var command = request.Adapt<UpdateTagValue.Command>();

            var result = await sender.Send(command);

            if (result.IsFailure)
            {
                return Results.BadRequest((object?)result.Error);
            }

            return Results.Ok((object?)result.Value);
        });
        app.MapPut("api/tag/putStrValue/",
            async (UpdateTagValue.Request1 request, ISender sender, TagService tagService) =>
            {
                await tagService.AddOrUpdateTagAsync(request.TagCode, request.Value);
            });
        app.MapGet("api/tag/updateStrValue", async (string tagCode, string value,
            ISender sender, TagService tagService) =>
        {
            try
            {
                await tagService.AddOrUpdateTagAsync(tagCode, value);
                return Results.NoContent(); // 返回 204 No Content 表示成功
            }
            catch (Exception ex)
            {
                // 记录错误日志
                Console.WriteLine($"更新标签失败: {ex}");
                return Results.Problem(statusCode: 500, title: "更新标签失败", detail: ex.Message); // 返回 500 错误
            }
        });
        app.MapGet("api/tag/updateStrValuePlc", async (string tagCode, string value,
            ISender sender, ITagRepository tagRepository, IDriverRepository _iDriverRepository,
            IEdgeDriverFactory edgeDriverFactory, TagService tagService) =>
        {
            try
            {
                await tagService.AddOrUpdateTagAsync(tagCode, value);
                Console.WriteLine($"更新标签到数据库前: {JsonConvert.SerializeObject(value)}");
                var tag = await tagRepository.GetQuery().FirstOrDefaultAsync(x => x.TagCode == tagCode);
                Console.WriteLine($"更新标签到数据库后: {JsonConvert.SerializeObject(tag.Value.GetValue().ToString())}");
                if (tag.DriverCode is not null)
                {
                    var driver = await _iDriverRepository.GetQuery()
                        .FirstOrDefaultAsync(d => d.DriverCode == tag.DriverCode);
                    //根据这个DriverConf 获取 Dirver
                    if (driver is not null && edgeDriverFactory.GetDrivers().ContainsKey(driver.DriverCode))
                    {
                        try
                        {
                            edgeDriverFactory.GetDrivers()[driver.DriverCode].DoWrite([tag]);
                            Console.WriteLine($"已经更新标签到PLC");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"更新标签到PLC失败: {e.Message}");
                        }
                    }

                    return Results.NoContent(); // 返回 204 No Content 表示成功
                }
                else
                {
                    // 记录错误日志
                    return Results.Problem(statusCode: 500, title: "更新PLC标签失败", detail: "找不到对应的连接器驱动"); // 返回 500 错误
                }
            }
            catch (Exception ex)
            {
                // 记录错误日志
                Console.WriteLine($"更新PLC标签失败: {ex}");
                return Results.Problem(statusCode: 500, title: "更新PLC标签失败", detail: ex.Message); // 返回 500 错误
            }

        });
    }
}