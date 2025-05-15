using Carter;
using ContentPlatform.Api.Busi.Logic.Common;
using ContentPlatform.Api.Busi.Logic.EdgeDriver;
using ContentPlatform.Api.Busi.Logic.Enums;
using ContentPlatform.Api.Database;
using ContentPlatform.Api.Entities;
using ContentPlatform.Api.Repository.Driver;
using Contracts;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Shared;

namespace ContentPlatform.Api.Tags;

public static class ReadTag
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
        private readonly IDriverRepository driverRepository;
        private readonly HybridCache _hybridCache;
        private readonly IEdgeDriverFactory edgeDriverFactory;

        public Handler(ApplicationDbContext dbContext, IPublishEndpoint publishEndpoint, Constants.EdgeDriverResolver edgeDriverResolver, IDriverRepository driverRepository, HybridCache hybridCache, IEdgeDriverFactory edgeDriverFactory)
        {
            _dbContext = dbContext;
            _publishEndpoint = publishEndpoint;
            this.edgeDriverResolver = edgeDriverResolver;
            this.driverRepository = driverRepository;
            _hybridCache = hybridCache;
            this.edgeDriverFactory = edgeDriverFactory;
        }

        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tag = await _dbContext
                .Tags
                .AsNoTracking()
                .FirstOrDefaultAsync(tag => tag.Id == request.Id, cancellationToken: cancellationToken);

            if (tag is null)
            {
                return Result.Failure<Response>(new Error(
                    "ReadTag.Null",
                    "The tag with the specified ID was not found"));
            }

            if (tag.DriverCode is null)
            {
                return Result.Failure<Response>(new Error(
                    "ReadTag.DriverCode.Null",
                    "driver code was not found"));
            }

            var drivers = edgeDriverFactory.GetDrivers();
            if (drivers.ContainsKey(tag.DriverCode))
            {
                edgeDriverFactory.GetDrivers()[tag.DriverCode].DoRead([tag]);
                await _publishEndpoint.Publish(
                    new TagReadedEvent(tag.Id,DateTime.UtcNow),
                    cancellationToken);
            }
            else
            {
                return Result.Failure<Response>(new Error(
                    "Running Driver NotExist ReadTag.DriverCode ",
                    "driver was not found in running Driver"));
            }

            var tagResponse = new Response() { Id = tag.Id };
            await _hybridCache.RemoveAsync("tags", cancellationToken);
            return tagResponse;
        }
    }
}

public class ReadTagEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/tag/read/{id}", async (Guid id, ISender sender) =>
        {
            var query = new ReadTag.Query { Id = id };

            var result = await sender.Send(query);

            if (result.IsFailure)
            {
                return Results.NotFound(result.Error);
            }

            return Results.Ok(result.Value);
        });
    }
}
