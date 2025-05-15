using Carter;
using ContentPlatform.Api.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Shared;

namespace ContentPlatform.Api.Busi.Driver.Api;

public static class GetDrivers
{
    public class Query : IRequest<Result<List<Response>>>;

    public class Response
    {
        public Guid Id { get; set; }

   
        public string DriverCode { get; set; }
        public int DriverType { get; set; }
        public string MachineCode { get; set; }
        public string ServerName { get; set; }
        public string ServerUrl { get; set; }
        public bool HasIdentity { get; set; }
        public string UserName { get; set; }
        public string PassWord { get; set; }
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
            var driverResponse = await _hybridCache.GetOrCreateAsync("drivers", async (ct) =>
            {
                return await _dbContext.Drivers.AsNoTracking()
                    .Select(driver => new Response
                    {
                        Id = driver.Id,
                        DriverCode=driver.DriverCode,
                        DriverType=driver.DriverType,
                        MachineCode=driver.MachineCode,
                        ServerName=driver.ServerName,
                        ServerUrl=driver.ServerUrl,
                        HasIdentity=driver.HasIdentity,
                        UserName=driver.UserName,
                        PassWord=driver.PassWord,
                        CreateTime=driver.CreateTime,
                        UpdateTime=driver.UpdateTime,
                    })
                    .ToListAsync();
            },cancellationToken: cancellationToken);
            return driverResponse;
        }
    }
}

public class GetDriversEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/drivers", async (ISender sender) =>
        {
            var query = new GetDrivers.Query();

            var result = await sender.Send(query);

            if (result.IsFailure)
            {
                return Results.NotFound((object?)result.Error);
            }

            return Results.Ok((object?)result.Value);
        });
    }
}
