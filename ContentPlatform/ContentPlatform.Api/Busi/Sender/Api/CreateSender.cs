using Carter;
using ContentPlatform.Api.Database;
using ContentPlatform.Api.Entities;
using Contracts;
using FluentValidation;
using Mapster;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared;

namespace ContentPlatform.Api.Busi.Sender.Api;

public static class CreateSender
{
    public class Request
    {
        public string SenderCode { get; set; }
        public string? MachineCode { get; set; }
        public string? DriverCode { get; set; }
        public int SenderType { get; set; }
        public Dictionary<string,string> Options { get; set; }
        public string? Desc { get; set; }
    }

    public class Command : IRequest<Result<Guid>>
    {
        public string SenderCode { get; set; }
        public string? MachineCode { get; set; }
        public string? DriverCode { get; set; }
        public int SenderType { get; set; }
        public Dictionary<string,string> Options { get; set; }
        public string? Desc { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.SenderCode).NotEmpty();
            RuleFor(c => c.SenderType).NotEmpty();
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
                    "CreateSender.Validation",
                    validationResult.ToString()));
            }

            var sender = new SenderEntity()
            {
                Id = Guid.NewGuid(),
            };


            sender = request.Adapt<SenderEntity>();
            sender.OptionsJson=JsonConvert.SerializeObject(request.Options);
            _dbContext.Add(sender);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(
                new SenderCreatedEvent(
                    sender.Id,
                    sender.SenderCode,
                    sender.MachineCode,
                    sender.DriverCode,
                    sender.SenderType,
                    sender.Options,
                    sender.Desc
                ),
                cancellationToken);
            await _hybridCache.RemoveAsync("senders", cancellationToken);
            return sender.Id;
        }
    }
}

public class CreateSenderEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/senders", async (CreateSender.Request request, ISender sender) =>
        {
            var command = request.Adapt<CreateSender.Command>();

            var result = await sender.Send(command);

            if (result.IsFailure)
            {
                return Results.BadRequest((object?)result.Error);
            }

            return Results.Ok((object?)result.Value);
        });
    }
}