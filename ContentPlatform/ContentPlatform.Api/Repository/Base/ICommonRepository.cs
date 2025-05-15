namespace ContentPlatform.Api.Repository;

public interface ICommonRepository<T>
{
    // IQueryable<T> GetListQuery();
    IQueryable<T> GetQuery(bool isTracking = false);

    Task CreateAsync(T entity);
    Task DeleteEntity(T entity, bool isSoftDelete = true);
    Task SaveChangesAsync();

    IQueryable<T> GetListQuery<TFilterModel>(TFilterModel filter,
        bool isTracking = false) where TFilterModel : class;
}