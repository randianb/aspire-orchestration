using ContentPlatform.Api.Database;
using ContentPlatform.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Tipa.EFCoreExtend;

namespace ContentPlatform.Api.Repository.Driver;

public class DriverRepository: IDriverRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DriverRepository> _logger;
    private readonly CommonQuery _commonQuery;

    public DriverRepository(ApplicationDbContext dbContext, ILogger<DriverRepository> logger,
        CommonQuery commonQuery)
    {
        _dbContext = dbContext;
        _logger = logger;
        _commonQuery = commonQuery;
    }

    public IQueryable<DriverEntity> GetQuery(bool isTracking = false)
    {
        return _commonQuery.GetQuery<DriverEntity>(_dbContext, isTracking);
    }

    public async Task CreateAsync(DriverEntity entity)
    {
        await _dbContext.Set<DriverEntity>().AddAsync(entity);
    }

    public async Task DeleteEntity(DriverEntity entity, bool isSoftDelete = true)
    {
        await _commonQuery.DeleteEntity(_dbContext, entity, isSoftDelete);
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }

    public IQueryable<DriverEntity> GetListQuery<TFilterModel>(TFilterModel filter, bool isTracking = false)
        where TFilterModel : class
    {
        var query =
            _commonQuery.GetListQuery<DriverEntity, TFilterModel>(
                filter,
                _dbContext);
        return isTracking ? query : query.AsNoTracking();
    }
}