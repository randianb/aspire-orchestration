using Carter;
using ContentPlatform.Api.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Newtonsoft.Json;
using Shared;

namespace ContentPlatform.Api.Articles;

public static class GetArticles
{
    public class Query : IRequest<Result<List<Response>>>;

    public class Response
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public List<string> Tags { get; set; } = new();

        public DateTime CreatedOnUtc { get; set; }

        public DateTime? PublishedOnUtc { get; set; }
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
            var articleResponse = await _hybridCache.GetOrCreateAsync("articles", async (ct) =>
            {
                return await _dbContext.Articles.AsNoTracking()
                    .Select(article => new Response
                    {
                        Id = article.Id,
                        Title = article.Title,
                        Content = article.Content,
                        Tags = article.Tags,
                        CreatedOnUtc = article.CreatedOnUtc,
                        PublishedOnUtc = article.PublishedOnUtc
                    })
                    .ToListAsync();
            },cancellationToken: cancellationToken);
            return articleResponse;
        }
    }
}

public class GetArticlesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/articles", async (ISender sender) =>
        {
            var query = new GetArticles.Query();

            var result = await sender.Send(query);

            if (result.IsFailure)
            {
                return Results.NotFound(result.Error);
            }

            return Results.Ok(result.Value);
        });
    }
}
