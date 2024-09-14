using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PuppeteerSharp;

namespace MidjourneyAPI.Core.Headless;

public static class BrowserContextExtensions
{
    public static IServiceCollection AddBrowserContextPool(this IServiceCollection services,
        int maxContexts, bool lazyInitialization = true)
    {
        var contextPool = new BrowserContextPool(maxContexts);

        services.AddSingleton(contextPool);

        if (!lazyInitialization)
        {
            contextPool.LaunchAsync(false).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        return services;
    }

    public static async Task<IPage> DefaultPageAsync(this IBrowserContext context)
    {
        var pages = await context.PagesAsync();
        return pages.FirstOrDefault() ?? await context.NewPageAsync();
    }
}