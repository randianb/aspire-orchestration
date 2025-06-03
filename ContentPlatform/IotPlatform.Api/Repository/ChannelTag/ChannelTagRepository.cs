using IotPlatform.Api.Database;
using IotPlatform.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Tipa.EFCoreExtend;

namespace IotPlatform.Api.Repository.Channel;

public class ChannelTagRepository: IChannelTagRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ChannelTagRepository> _logger;
    private readonly CommonQuery _commonQuery;

    public ChannelTagRepository(ApplicationDbContext dbContext, ILogger<ChannelTagRepository> logger,
        CommonQuery commonQuery)
    {
        _dbContext = dbContext;
        _logger = logger;
        _commonQuery = commonQuery;
    }

    public IQueryable<ChannelTagEntity> GetQuery(bool isTracking = false)
    {
        return _commonQuery.GetQuery<ChannelTagEntity>(_dbContext, isTracking);
    }

    public async Task CreateAsync(ChannelTagEntity entity)
    {
        await _dbContext.Set<ChannelTagEntity>().AddAsync(entity);
    }

    public async Task DeleteEntity(ChannelTagEntity entity, bool isSoftDelete = true)
    {
        await _commonQuery.DeleteEntity(_dbContext, entity, isSoftDelete);
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }

    public IQueryable<ChannelTagEntity> GetListQuery<TFilterModel>(TFilterModel filter, bool isTracking = false)
        where TFilterModel : class
    {
        var query =
            _commonQuery.GetListQuery<ChannelTagEntity, TFilterModel>(
                filter,
                _dbContext);
        return isTracking ? query : query.AsNoTracking();
    }
}