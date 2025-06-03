using Carter;
using IotPlatform.Api.Database;
using IotPlatform.Api.Repository;
using Contracts;
using FluentValidation;
using IotPlatform.Api.Entities;
using IotPlatform.Api.Repository.Tag;
using Mapster;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Shared;

namespace IotPlatform.Api.Busi.Tag.Api;

public static class UpdateTag
{
    public class Request
    {
        public Guid Id { get; set; }
        public string? GroupCode { get; set; }
        public string? DriverCode { get; set; }
        public string? EquipCode { get; set; }
        public string TagCode { get; set; }
        public string DataType { get; set; }
        public string? Desc { get; set; }
        public ObjValue? Value { get; set; }
    }

    public class Command : IRequest<Result<Guid>>
    {
        public Guid Id { get; set; }
        public string? GroupCode { get; set; }
        public string? DriverCode { get; set; }
        public string? EquipCode { get; set; }
        public string TagCode { get; set; }
        public string DataType { get; set; }
        public string? Desc { get; set; }
        public ObjValue? Value { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.TagCode).NotEmpty();
            RuleFor(c => c.DataType).NotEmpty();
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
                    "UpdateTag.Validation",
                    validationResult.ToString()));
            }

            var tag = await _iTagRepository.GetQuery(true).FirstOrDefaultAsync(x => x.TagCode == request.TagCode,
                cancellationToken: cancellationToken);
            if (tag == null)
            {
                return Result.Failure<Guid>(new Error(
                    "UpdateTag.Validation",
                    "not found"));
            }

            tag = request.Adapt<TagEntity>();
            tag.UpdateTime = DateTime.UtcNow;

            await _iTagRepository.SaveChangesAsync();

            await _publishEndpoint.Publish(
                new TagUpdatedEvent(tag.Id,  tag.GroupCode,
                    tag.DriverCode,
                    tag.EquipCode,
                    tag.TagCode,
                    tag.DataType,
                    tag.Desc,
                    tag.Value is null ?null: tag.Value.GetValue().ToString()),
                cancellationToken);
            await _hybridCache.RemoveAsync("tags", cancellationToken);
            return tag.Id;
        }
    }
}

public class UpdateTagEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("api/tags", async (UpdateTag.Request request, ISender sender) =>
        {
            var command = request.Adapt<UpdateTag.Command>();

            var result = await sender.Send(command);

            if (result.IsFailure)
            {
                return Results.BadRequest((object?)result.Error);
            }

            return Results.Ok((object?)result.Value);
        });
    }
}