using ContentPlatform.Api.Database;
using ContentPlatform.Api.Entities;
using ContentPlatform.Api.Repository.Tag;
using Microsoft.EntityFrameworkCore;
using Tipa.EFCoreExtend;

namespace ContentPlatform.Api.Repository.RequestResponseLog;

public class RequestResponseLogRepository: IRequestResponseLogRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<RequestResponseLogRepository> _logger;
    private readonly CommonQuery _commonQuery;

    public RequestResponseLogRepository(ApplicationDbContext dbContext, ILogger<RequestResponseLogRepository> logger,
        CommonQuery commonQuery)
    {
        _dbContext = dbContext;
        _logger = logger;
        _commonQuery = commonQuery;
    }

    public IQueryable<RequestResponseLogEntity> GetQuery(bool isTracking = false)
    {
        return _commonQuery.GetQuery<RequestResponseLogEntity>(_dbContext, isTracking);
    }

    public async Task CreateAsync(RequestResponseLogEntity entity)
    {
        await _dbContext.Set<RequestResponseLogEntity>().AddAsync(entity);
    }

    public async Task DeleteEntity(RequestResponseLogEntity entity, bool isSoftDelete = true)
    {
        await _commonQuery.DeleteEntity(_dbContext, entity, isSoftDelete);
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }

    public IQueryable<RequestResponseLogEntity> GetListQuery<TFilterModel>(TFilterModel filter, bool isTracking = false)
        where TFilterModel : class
    {
        var query =
            _commonQuery.GetListQuery<RequestResponseLogEntity, TFilterModel>(
                filter,
                _dbContext);
        return isTracking ? query : query.AsNoTracking();
    }
}