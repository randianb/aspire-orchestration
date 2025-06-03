using IotPlatform.Api.Database;
using IotPlatform.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Tipa.EFCoreExtend;

namespace IotPlatform.Api.Repository.Channel;

public class ChannelTagHistoryRepository: IChannelTagHistoryRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ChannelTagHistoryRepository> _logger;
    private readonly CommonQuery _commonQuery;

    public ChannelTagHistoryRepository(ApplicationDbContext dbContext, ILogger<ChannelTagHistoryRepository> logger,
        CommonQuery commonQuery)
    {
        _dbContext = dbContext;
        _logger = logger;
        _commonQuery = commonQuery;
    }

    public IQueryable<ChannelTagHistoryEntity> GetQuery(bool isTracking = false)
    {
        return _commonQuery.GetQuery<ChannelTagHistoryEntity>(_dbContext, isTracking);
    }

    public async Task CreateAsync(ChannelTagHistoryEntity entity)
    {
        await _dbContext.Set<ChannelTagHistoryEntity>().AddAsync(entity);
    }

    public async Task DeleteEntity(ChannelTagHistoryEntity entity, bool isSoftDelete = true)
    {
        await _commonQuery.DeleteEntity(_dbContext, entity, isSoftDelete);
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }

    public IQueryable<ChannelTagHistoryEntity> GetListQuery<TFilterModel>(TFilterModel filter, bool isTracking = false)
        where TFilterModel : class
    {
        var query =
            _commonQuery.GetListQuery<ChannelTagHistoryEntity, TFilterModel>(
                filter,
                _dbContext);
        return isTracking ? query : query.AsNoTracking();
    }
}