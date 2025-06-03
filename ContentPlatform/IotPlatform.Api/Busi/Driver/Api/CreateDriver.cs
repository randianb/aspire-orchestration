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

namespace IotPlatform.Api.Busi.Driver.Api;

public static class CreateDriver
{
    public class Request
    {
        public string DriverCode { get; set; }
        public int DriverType { get; set; }
        public string MachineCode { get; set; }
        public string ServerName { get; set; }
        public string ServerUrl { get; set; }
        public bool HasIdentity { get; set; }
        public string UserName { get; set; }
        public string PassWord { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }

    public class Command : IRequest<Result<Guid>>
    {
        public string DriverCode { get; set; }
        public int DriverType { get; set; }
        public string MachineCode { get; set; }
        public string ServerName { get; set; }
        public string ServerUrl { get; set; }
        public bool HasIdentity { get; set; }
        public string UserName { get; set; }
        public string PassWord { get; set; }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(c => c.DriverCode).NotEmpty();
                RuleFor(c => c.DriverType).NotEmpty();
                RuleFor(c => c.ServerName).NotEmpty();
            }
        }

        internal sealed class Handler : IRequestHandler<Command, Result<Guid>>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly IValidator<Command> _validator;
            private readonly IPublishEndpoint _publishEndpoint;
            private readonly HybridCache _hybridCache;

            public Handler(ApplicationDbContext dbContext, IValidator<Command> validator,
                IPublishEndpoint publishEndpoint, HybridCache hybridCache)
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
                        "CreateDriver.Validation",
                        validationResult.ToString()));
                }

                var driver = new DriverEntity()
                {
                    Id = Guid.NewGuid(),
                };
                driver = request.Adapt<DriverEntity>();
                _dbContext.Add(driver);

                await _dbContext.SaveChangesAsync(cancellationToken);

                await _publishEndpoint.Publish(
                    new DriverCreatedEvent(driver.Id,
                        driver.DriverCode,
                        driver.DriverType,
                        driver.MachineCode,
                        driver.ServerName,
                        driver.ServerUrl,
                        driver.HasIdentity,
                        driver.UserName,
                        driver.PassWord
                    ),
                    cancellationToken);
                await _hybridCache.RemoveAsync("drivers", cancellationToken);
                return driver.Id;
            }
        }

        public class CreateDriverEndpoint : ICarterModule
        {
            public void AddRoutes(IEndpointRouteBuilder app)
            {
                app.MapPost("api/drivers", async (CreateDriver.Request request, ISender sender) =>
                {
                    var command = request.Adapt<CreateDriver.Command>();

                    var result = await sender.Send(command);

                    if (result.IsFailure)
                    {
                        return Results.BadRequest((object?)result.Error);
                    }

                    return Results.Ok((object?)result.Value);
                });
            }
        }
    }
}