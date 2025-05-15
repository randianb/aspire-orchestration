using AutoMapper.Internal;
using ContentPlatform.Api.Repository;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ContentPlatform.Api;

public static class ProgramExtensions
{
    public static void ConfigureRepository(this IServiceCollection services)
    {
        var assembly = typeof(ProgramExtensions).Assembly;
        var typeInfos = assembly.DefinedTypes
            .Where(o => o.IsClass && o is { IsAbstract: false, IsGenericType: false, Namespace: not null });
        // 动态添加Query中的Repository，继承了ICommonQuery接口
        foreach (var type in typeInfos)
        {
            foreach (var @interface in type.GetInterfaces())
            {
                if (@interface != typeof(ICommonRepository<>) &&
                    @interface.GetGenericInterface(typeof(ICommonRepository<>)) != null)
                {
                    services.TryAddScoped(@interface, type);
                }
            }
        }
    }

}