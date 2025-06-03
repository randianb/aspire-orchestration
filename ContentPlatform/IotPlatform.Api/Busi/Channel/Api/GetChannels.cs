using Carter;
using IotPlatform.Api.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Shared;

namespace IotPlatform.Api.Busi.Channel.Api;

public static class GetChannels
{
    public class Query : IRequest<Result<List<Response>>>;

    public class Response
    {
        public Guid Id { get; set; }

        public string ChannelCode { get; set; }
        public bool IsSchedule { get; set; } = false;
        public string Topic { get; set; }
        public string Desc { get; set; }
        public List<string>  SenderCodes { get; set; }= new();
        
        public List<string> TagCodes { get; set; } = new();
    }

    internal sealed class Handler : IRequestHandler<Query, Result<List<Response>>>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly HybridCache _hybridCache;
        private readonly IDistributedCache distributedCache;

        public Handler(ApplicationDbContext dbContext, HybridCache hybridCache, IDistributedCache distributedCache)
        {
            _dbContext = dbContext;
            _hybridCache = hybridCache;
            this.distributedCache = distributedCache;
        }

        public async Task<Result<List<Response>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var channelResponse = await _hybridCache.GetOrCreateAsync("channels", async (ct) =>
            {
                return await _dbContext.Channels.AsNoTracking()
                    .Select(channel => new Response
                    {
                        Id = channel.Id,
                        ChannelCode = channel.ChannelCode,
                        Topic = channel.Topic,
                        IsSchedule = channel.IsSchedule,
                        Desc = channel.Desc,
                        SenderCodes = channel.SenderCodes,
                        TagCodes = channel.TagCodes
                    })
                    .ToListAsync();
            }, cancellationToken: cancellationToken);
            return channelResponse;
        }
    }
}

public class GetChannelsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/channels", async (ISender sender) =>
        {
            var query = new GetChannels.Query();

            var result = await sender.Send(query);

            if (result.IsFailure)
            {
                return Results.NotFound((object?)result.Error);
            }

            return Results.Ok((object?)result.Value);
        });
    }
}