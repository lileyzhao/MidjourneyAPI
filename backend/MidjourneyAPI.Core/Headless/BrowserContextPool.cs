using System.Text.Json;
using PuppeteerSharp;

namespace MidjourneyAPI.Core.Headless;

/// <summary>
/// PuppeteerSharp 的 IBrowserContext 上下文实例池。
/// </summary>
public class BrowserContextPool : IAsyncDisposable
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>
    /// 初始化 BrowserContextPoolManager 类的新实例。
    /// </summary>
    /// <param name="maxContexts">上下文池的最大上下文数量。</param>
    public BrowserContextPool(int maxContexts)
    {
        BrowserContextCollection = new BrowserContextCollection();
        MaxContexts = maxContexts;
    }

    /// <summary>
    /// 浏览器实例。
    /// </summary>
    public IBrowser? Browser { get; private set; }

    /// <summary>
    /// 浏览器上下文集合。
    /// </summary>
    public BrowserContextCollection BrowserContextCollection { get; }

    /// <summary>
    /// 最大上下文数量。
    /// </summary>
    public int MaxContexts { get; private set; }

    /// <summary>
    /// 实现 IAsyncDisposable 接口，用于异步释放资源。
    /// </summary>
    /// <returns></returns>
    public async ValueTask DisposeAsync()
    {
        if (Browser != null)
        {
            await ClearContextAsync();

            await Browser.CloseAsync();
            Browser.Dispose();
            Browser = null;
        }

        _semaphore.Dispose();
    }

    public async Task<bool> SetMaxContextsAsync(int maxContexts)
    {
        if (maxContexts <= 0) return false;

        try
        {
            await _semaphore.WaitAsync();

            while (BrowserContextCollection.Count > maxContexts)
            {
                var firstKey = BrowserContextCollection.Keys.FirstOrDefault();
                if (firstKey == null) continue;
                BrowserContextCollection.Remove(firstKey, out var removedContext);
                if (removedContext == null) continue;
                await removedContext.CloseAsync();
                Console.WriteLine($"Removed the oldest browser context with key For `SetMaxContextsAsync`: {firstKey}");
            }

            MaxContexts = maxContexts;
        }
        finally
        {
            _semaphore.Release();
        }

        return true;
    }

    /// <summary>
    /// 启动浏览器上下文池
    /// </summary>
    /// <returns></returns>
    public async Task LaunchAsync(bool headless = true)
    {
        if (Browser != null) return;

        try
        {
            await _semaphore.WaitAsync();

            // 下载并设置浏览器
            var brs = await new BrowserFetcher().DownloadAsync();
            Console.WriteLine($"Browser downloaded to {JsonSerializer.Serialize(brs)}");
        }
        finally
        {
            _semaphore.Release();
        }

        // 启动无头浏览器
        Browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = headless,
            Args = ["--disable-dev-shm-usage"] // 避免使用共享内存
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// 从上下文池中获取一个 PuppeteerSharp 的 IBrowserContext 实例。
    /// 如果池中没有可用的上下文，则创建一个新的上下文。
    /// 如果池已满，则移除第一个上下文，并将新创建的上下文添加进去。
    /// </summary>
    /// <param name="userId">用户的唯一标识。</param>
    /// <returns>一个 PuppeteerSharp 的 IBrowserContext 实例。</returns>
    public async Task<IBrowserContext> GetContextAsync(string userId)
    {
        if (Browser == null)
            throw new NullReferenceException("浏览器实例为 Null, 请先运行 BrowserContextPool.Launch() 方法创建浏览器实例");

        try
        {
            await _semaphore.WaitAsync();

            var context = BrowserContextCollection.Get(userId);
            if (context != null) return context;

            if (BrowserContextCollection.Count >= MaxContexts)
            {
                // 移除第一个上下文
                var firstKey = BrowserContextCollection.Keys.FirstOrDefault();
                if (firstKey != null)
                {
                    BrowserContextCollection.Remove(firstKey, out var removedContext);
                    if (removedContext != null) await removedContext.CloseAsync();
                    Console.WriteLine($"Removed the oldest browser context with key: {firstKey}");
                }
            }

            context = await Browser.CreateBrowserContextAsync();
            Console.WriteLine($"Created a new browser context for user: {userId}");

            // 添加新创建的上下文
            BrowserContextCollection.Add(userId, context);
            Console.WriteLine($"Added a new browser context with key: {userId}");

            return context;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to create a new browser context for user {userId}: {ex.Message}");
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    /// <summary>
    /// 从上下文池中获取一个 PuppeteerSharp 的 IBrowserContext 实例。
    /// 如果池中没有可用的上下文，则创建一个新的上下文。
    /// 如果池已满，则移除第一个上下文，并将新创建的上下文添加进去。
    /// </summary>
    /// <returns>一个 PuppeteerSharp 的 IBrowserContext 实例。</returns>
    public async Task<IBrowserContext?> GetNextContextAsync()
    {
        if (Browser == null)
            throw new NullReferenceException("浏览器实例为 Null, 请先运行 BrowserContextPool.Launch() 方法创建浏览器实例");

        try
        {
            await _semaphore.WaitAsync();

            return BrowserContextCollection.GetNext();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to get next browser context: {ex.Message}");
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<IPage> GetDefaultPageByContextAsync(string userId)
    {
        var context = await GetContextAsync(userId);
        var pages = await context.PagesAsync();
        return pages.FirstOrDefault() ?? await context.NewPageAsync();
    }

    /// <summary>
    /// 清空上下文池中的所有 PuppeteerSharp 的 IBrowserContext 实例。
    /// </summary>
    public async Task ClearContextAsync()
    {
        if (Browser == null)
            throw new NullReferenceException("浏览器实例为 Null, 请先运行 BrowserContextPool.Launch() 方法创建浏览器实例");

        try
        {
            await _semaphore.WaitAsync();

            foreach (var context in BrowserContextCollection.Values)
            {
                if (context.IsClosed) continue;
                await context.CloseAsync();
            }

            BrowserContextCollection.Clear();
            Console.WriteLine("Cleared all browser contexts.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to clear all browser context: {ex.Message}");
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}