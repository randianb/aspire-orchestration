using Carter;
using ContentPlatform.Api.Database;
using ContentPlatform.Api.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Shared;

namespace ContentPlatform.Api.Busi.Tag.Api;

public static class GetTags
{
    public class Query : IRequest<Result<List<Response>>>;

    public class Response
    {
        public Guid Id { get; set; }
        public string? DriverCode { get; set; }
        public string? GroupCode { get; set; }
        public string? EquipCode { get; set; }
        public string TagCode { get; set; }
        public string DataType { get; set; }
        public string? Value { get; set; }
        public string? Desc { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
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
            var tagResponse = await _hybridCache.GetOrCreateAsync("tags", async (ct) =>
            {
                return await _dbContext.Tags.AsNoTracking()
                    .Select(tag => new Response
                    {
                        Id = tag.Id,
                        GroupCode = tag.GroupCode,
                        DriverCode = tag.DriverCode,
                        EquipCode = tag.EquipCode,
                        TagCode = tag.TagCode,
                        DataType = tag.DataType,
                        Desc = tag.Desc,
                        Value = tag.Value==null?null:tag.Value.GetValue().ToString(),
                    })
                    .ToListAsync();
            }, cancellationToken: cancellationToken);
            return tagResponse;
        }
    }
}

public class GetTagsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/tags", async (ISender sender) =>
        {
            var query = new GetTags.Query();

            var result = await sender.Send(query);

            if (result.IsFailure)
            {
                return Results.NotFound((object?)result.Error);
            }

            return Results.Ok((object?)result.Value);
        });
    }
}