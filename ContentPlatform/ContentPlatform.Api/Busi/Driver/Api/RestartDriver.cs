using Carter;
using ContentPlatform.Api.Busi.Logic.Common;
using ContentPlatform.Api.Busi.Logic.EdgeDriver;
using ContentPlatform.Api.Busi.Logic.Enums;
using ContentPlatform.Api.Database;
using ContentPlatform.Api.Entities;
using Contracts;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared;

namespace ContentPlatform.Api.Drivers;

public static class RestartDriver
{
    public class Query : IRequest<Result<Response>>
    {
        public Guid Id { get; set; }
    }

    public class Response
    {
        public Guid Id { get; set; }

    }

    internal sealed class Handler : IRequestHandler<Query, Result<Response>>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly Constants.EdgeDriverResolver edgeDriverResolver;
        private readonly IEdgeDriverFactory edgeDriverFactory;

        public Handler(ApplicationDbContext dbContext, IPublishEndpoint publishEndpoint, Constants.EdgeDriverResolver edgeDriverResolver, IEdgeDriverFactory edgeDriverFactory)
        {
            _dbContext = dbContext;
            _publishEndpoint = publishEndpoint;
            this.edgeDriverResolver = edgeDriverResolver;
            this.edgeDriverFactory = edgeDriverFactory;
        }

        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var driver = await _dbContext
                .Drivers
                .AsNoTracking()
                .FirstOrDefaultAsync(driver => driver.Id == request.Id, cancellationToken: cancellationToken);

            if (driver is null)
            {
                return Result.Failure<Response>(new Error(
                    "RestartDriver.Null",
                    "The driver with the specified ID was not found"));
            } 
            var edgeDriver = edgeDriverResolver((DriverTypeEnum)driver.DriverType);
            edgeDriverFactory.GetDrivers().Add(driver.DriverCode,edgeDriver);
            edgeDriver.Stop();
            edgeDriver.Run(driver);
            await _publishEndpoint.Publish(
                new DriverRestartedEvent(driver.Id,DateTime.UtcNow),
                cancellationToken);
            var driverResponse = new Response() { Id = driver.Id };
            return driverResponse;
        }
    }
}

public class RestartDriverEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/driver/restart/{id}", async (Guid id, ISender sender) =>
        {
            var query = new RestartDriver.Query { Id = id };

            var result = await sender.Send(query);

            if (result.IsFailure)
            {
                return Results.NotFound(result.Error);
            }

            return Results.Ok(result.Value);
        });
    }
}
