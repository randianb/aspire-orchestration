using ContentPlatform.Api.Database;
using ContentPlatform.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Tipa.EFCoreExtend;

namespace ContentPlatform.Api.Repository.Channel;

public class ChannelRepository: IChannelRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ChannelRepository> _logger;
    private readonly CommonQuery _commonQuery;

    public ChannelRepository(ApplicationDbContext dbContext, ILogger<ChannelRepository> logger,
        CommonQuery commonQuery)
    {
        _dbContext = dbContext;
        _logger = logger;
        _commonQuery = commonQuery;
    }

    public IQueryable<ChannelEntity> GetQuery(bool isTracking = false)
    {
        return _commonQuery.GetQuery<ChannelEntity>(_dbContext, isTracking);
    }

    public async Task CreateAsync(ChannelEntity entity)
    {
        await _dbContext.Set<ChannelEntity>().AddAsync(entity);
    }

    public async Task DeleteEntity(ChannelEntity entity, bool isSoftDelete = true)
    {
        await _commonQuery.DeleteEntity(_dbContext, entity, isSoftDelete);
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }

    public IQueryable<ChannelEntity> GetListQuery<TFilterModel>(TFilterModel filter, bool isTracking = false)
        where TFilterModel : class
    {
        var query =
            _commonQuery.GetListQuery<ChannelEntity, TFilterModel>(
                filter,
                _dbContext);
        return isTracking ? query : query.AsNoTracking();
    }
}