using Carter;
using IotPlatform.Api.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared;
using Tipa.JsonExtern;

namespace IotPlatform.Api.Busi.Sender.Api;

public static class GetSenders
{
    public class Query : IRequest<Result<List<Response>>>;

    public class Response
    {
        public Guid Id { get; set; }

        public string SenderCode { get; set; }
        public string? MachineCode { get; set; }
        public string? DriverCode { get; set; }
        public int SenderType { get; set; }
        public Dictionary<string,string> Options { get; set; }
        public string? Desc { get; set; }
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
            var senderResponse = await _hybridCache.GetOrCreateAsync("senders", async (ct) =>
            {
                return await _dbContext.Senders.AsNoTracking()
                    .Select(sender => new Response
                    {
                        Id = sender.Id,
                        SenderCode = sender.SenderCode,
                        MachineCode = sender.MachineCode,
                        DriverCode = sender.DriverCode,
                        SenderType = sender.SenderType,
                        Options = JsonConvert.DeserializeObject<Dictionary<string,string>>(sender.Options.ToString()),
                        Desc = sender.Desc
                    })
                    .ToListAsync();
            }, cancellationToken: cancellationToken);
            return senderResponse;
        }
    }
}

public class GetSendersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/senders", async (ISender sender) =>
        {
            var query = new GetSenders.Query();

            var result = await sender.Send(query);

            if (result.IsFailure)
            {
                return Results.NotFound((object?)result.Error);
            }

            return Results.Ok((object?)result.Value);
        });
    }
}