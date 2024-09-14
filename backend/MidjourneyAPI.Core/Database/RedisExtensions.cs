using Microsoft.Extensions.DependencyInjection;
using NewLife.Caching;

namespace MidjourneyAPI.Core.Database;

public static class RedisExtensions
{
    /// <summary>
    /// 缓存注册（新生命Redis组件）
    /// </summary>
    /// <param name="services"></param>
    public static void AddRedisCache(this IServiceCollection services)
    {
        var cache = new FullRedis(new RedisOptions
        {
            Configuration =
                "r-xiix-out.redis.rds.aliyuncs.com:2579,password=Wpq2&_+N&#%)-AXS_EZ6PzT&-_eg((=^,db=5,timeout=3000,allowAdmin=true",
            Prefix = "MjApi"
        });

        services.AddSingleton(cache);
    }
}