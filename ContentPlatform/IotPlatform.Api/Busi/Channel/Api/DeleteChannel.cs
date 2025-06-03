using Carter;
using Contracts;
using IotPlatform.Api.Database;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Shared;

namespace IotPlatform.Api.Busi.Channel.Api;

public static class DeleteChannel
{
    public class Command : IRequest<Result>
    {
        public Guid Id { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Command, Result>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly HybridCache _hybridCache;
        public Handler(ApplicationDbContext dbContext, IPublishEndpoint publishEndpoint, HybridCache hybridCache)
        {
            _dbContext = dbContext;
            _publishEndpoint = publishEndpoint;
            _hybridCache = hybridCache;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var channel = await _dbContext
                .Channels
                .Where(channel => channel.Id == request.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (channel is null)
            {
                return Result.Failure(new Error(
                    "GetChannel.Null",
                    "The channel with the specified ID was not found"));
            }

            _dbContext.Remove(channel);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new ChannelDeletedEvent(channel.Id), cancellationToken);
            await _hybridCache.RemoveAsync("channels", cancellationToken);
            return Result.Success();
        }
    }
}

public class DeleteChannelEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/channels/{id}", async (Guid id, ISender sender) =>
        {
            var query = new DeleteChannel.Command { Id = id };

            var result = await sender.Send(query);

            if (result.IsFailure)
            {
                return Results.NotFound((object?)result.Error);
            }

            return Results.Ok();
        });
    }
}
