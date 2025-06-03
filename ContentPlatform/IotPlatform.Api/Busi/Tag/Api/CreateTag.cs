using Carter;
using Contracts;
using FluentValidation;
using IotPlatform.Api.Database;
using IotPlatform.Api.Entities;
using Mapster;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Shared;

namespace IotPlatform.Api.Busi.Tag.Api;

public static class CreateTag
{
    public class Request
    {
        public string? GroupCode { get; set; }
        public string? DriverCode { get; set; }
        public string? EquipCode { get; set; }
        public string TagCode { get; set; }
        public string DataType { get; set; }
        public string? Desc { get; set; }
    }

    public class Command : IRequest<Result<Guid>>
    {
        public string? GroupCode { get; set; }
        public string? DriverCode { get; set; }
        public string? EquipCode { get; set; }
        public string TagCode { get; set; }
        public string DataType { get; set; }
        public string? Desc { get; set; }
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
        private readonly ApplicationDbContext _dbContext;
        private readonly IValidator<Command> _validator;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly HybridCache _hybridCache;

        public Handler(ApplicationDbContext dbContext, IValidator<Command> validator, IPublishEndpoint publishEndpoint,
            HybridCache hybridCache)
        {
            _dbContext = dbContext;
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
                    "CreateTag.Validation",
                    validationResult.ToString()));
            }

            var tag = new TagEntity()
            {
                Id = Guid.NewGuid(),
                CreateTime = DateTime.UtcNow
            };
            tag = request.Adapt<TagEntity>();
            _dbContext.Add(tag);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _publishEndpoint.Publish(
                new TagCreatedEvent(tag.Id,
                    tag.GroupCode,
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

public class CreateTagEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/tags", async (CreateTag.Request request, ISender sender) =>
        {
            var command = request.Adapt<CreateTag.Command>();

            var result = await sender.Send(command);

            if (result.IsFailure)
            {
                return Results.BadRequest((object?)result.Error);
            }

            return Results.Ok((object?)result.Value);
        });
    }
}