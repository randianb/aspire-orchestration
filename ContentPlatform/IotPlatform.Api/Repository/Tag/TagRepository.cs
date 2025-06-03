using IotPlatform.Api.Database;
using IotPlatform.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Tipa.EFCoreExtend;

namespace IotPlatform.Api.Repository.Tag;

public class TagRepository: ITagRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<TagRepository> _logger;
    private readonly CommonQuery _commonQuery;

    public TagRepository(ApplicationDbContext dbContext, ILogger<TagRepository> logger,
        CommonQuery commonQuery)
    {
        _dbContext = dbContext;
        _logger = logger;
        _commonQuery = commonQuery;
    }

    public IQueryable<TagEntity> GetQuery(bool isTracking = false)
    {
        return _commonQuery.GetQuery<TagEntity>(_dbContext, isTracking);
    }

    public async Task CreateAsync(TagEntity entity)
    {
        await _dbContext.Set<TagEntity>().AddAsync(entity);
    }

    public async Task DeleteEntity(TagEntity entity, bool isSoftDelete = true)
    {
        await _commonQuery.DeleteEntity(_dbContext, entity, isSoftDelete);
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }

    public IQueryable<TagEntity> GetListQuery<TFilterModel>(TFilterModel filter, bool isTracking = false)
        where TFilterModel : class
    {
        var query =
            _commonQuery.GetListQuery<TagEntity, TFilterModel>(
                filter,
                _dbContext);
        return isTracking ? query : query.AsNoTracking();
    }
}