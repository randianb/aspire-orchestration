using Carter;
using ContentPlatform.Api.Database;
using ContentPlatform.Api.Entities;
using Contracts;
using FluentValidation;
using Mapster;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Shared;

namespace ContentPlatform.Api.Busi.Channel.Api;

public static class CreateChannel
{
    public class Request
    {
        public string ChannelCode { get; set; }
        public bool IsSchedule { get; set; } = false;
        public string Topic { get; set; }
        public string Desc { get; set; }
        public List<string> SenderCodes { get; set; } = new();

        public List<string> TagCodes { get; set; } = new();
    }

    public class Command : IRequest<Result<Guid>>
    {
        public string ChannelCode { get; set; }
        public bool IsSchedule { get; set; } = false;
        public string Topic { get; set; }
        public string Desc { get; set; }
        public List<string> SenderCodes { get; set; } = new();

        public List<string> TagCodes { get; set; } = new();
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.ChannelCode).NotEmpty();
            RuleFor(c => c.SenderCodes).NotEmpty();
            RuleFor(c => c.TagCodes).NotEmpty();
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
                    "CreateChannel.Validation",
                    validationResult.ToString()));
            }

            var channel = new ChannelEntity()
            {
                Id = Guid.NewGuid(),
            };
            channel = request.Adapt<ChannelEntity>();
            _dbContext.Add(channel);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(
                new ChannelCreatedEvent(
                    channel.Id,
                    channel.ChannelCode,
                    channel.IsSchedule,
                    channel.Topic,
                    channel.Desc,
                    channel.SenderCodes,
                    channel.TagCodes
                ),
                cancellationToken);
            await _hybridCache.RemoveAsync("channels", cancellationToken);
            return channel.Id;
        }
    }
}

public class CreateChannelEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/channels", async (CreateChannel.Request request, ISender sender) =>
        {
            var command = request.Adapt<CreateChannel.Command>();

            var result = await sender.Send(command);

            if (result.IsFailure)
            {
                return Results.BadRequest((object?)result.Error);
            }

            return Results.Ok((object?)result.Value);
        });
    }
}