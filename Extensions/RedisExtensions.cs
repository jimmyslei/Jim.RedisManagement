using Microsoft.Extensions.DependencyInjection;

#if !NETSTANDARD2_0
namespace Jim.RedisManagement;
#else
namespace Jim.RedisManagement {
#endif
public static class RedisExtensions
{
    /// <summary>
    /// 注册redis服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddRedis(this IServiceCollection services)
    {
        services.AddOptions<RedisOptions>()
            .BindConfiguration("Redis");
        services.AddSingleton<IRedisManage, RedisManage>();

        return services;
    }
}
#if NETSTANDARD2_0
}
#endif