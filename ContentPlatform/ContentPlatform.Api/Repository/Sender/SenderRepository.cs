using ContentPlatform.Api.Database;
using ContentPlatform.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Tipa.EFCoreExtend;

namespace ContentPlatform.Api.Repository.Sender;

public class SenderRepository: ISenderRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<SenderRepository> _logger;
    private readonly CommonQuery _commonQuery;

    public SenderRepository(ApplicationDbContext dbContext, ILogger<SenderRepository> logger,
        CommonQuery commonQuery)
    {
        _dbContext = dbContext;
        _logger = logger;
        _commonQuery = commonQuery;
    }

    public IQueryable<SenderEntity> GetQuery(bool isTracking = false)
    {
        return _commonQuery.GetQuery<SenderEntity>(_dbContext, isTracking);
    }

    public async Task CreateAsync(SenderEntity entity)
    {
        await _dbContext.Set<SenderEntity>().AddAsync(entity);
    }

    public async Task DeleteEntity(SenderEntity entity, bool isSoftDelete = true)
    {
        await _commonQuery.DeleteEntity(_dbContext, entity, isSoftDelete);
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }

    public IQueryable<SenderEntity> GetListQuery<TFilterModel>(TFilterModel filter, bool isTracking = false)
        where TFilterModel : class
    {
        var query =
            _commonQuery.GetListQuery<SenderEntity, TFilterModel>(
                filter,
                _dbContext);
        return isTracking ? query : query.AsNoTracking();
    }
}