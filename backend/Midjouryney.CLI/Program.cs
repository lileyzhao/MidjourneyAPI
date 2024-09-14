using System.Diagnostics;
using Furion;
using MidjourneyAPI.Core.Headless;
using PuppeteerSharp;

Serve.RunNative(services =>
{
    var sw = new Stopwatch();
    sw.Start();
    services.AddBrowserContextPool(3, false);
    sw.Stop();
    Console.WriteLine($"BrowserContextPool 初始化完成，耗时: {sw.ElapsedMilliseconds}ms");
});

var sw2 = new Stopwatch();
sw2.Start();
var brsContextPool = App.GetService<BrowserContextPool>();
for (var i = 0; i < 5; i++)
{
    var ctt = await brsContextPool.GetContextAsync(i.ToString());
    if (ctt == null) continue;
    var pages = await ctt.PagesAsync();
    var page = pages.FirstOrDefault() ?? await ctt.NewPageAsync();

    await page.GoToAsync("https://www.google.com",
        new NavigationOptions { WaitUntil = [WaitUntilNavigation.DOMContentLoaded] });

    // 忽略其他资源的加载
    // await page.SetRequestInterceptionAsync(true);
    // page.Request += (sender, e) =>
    // {
    //     if (e.Request.ResourceType != ResourceType.Document)
    //     {
    //         e.Request.AbortAsync();
    //     }
    //     else
    //     {
    //         e.Request.ContinueAsync();
    //     }
    // };
}

var cCount = brsContextPool.BrowserContextCollection.Count;
Console.WriteLine($"BrowserContextPool 中的 BrowserContext 数量: {cCount}个");

sw2.Stop();
Console.WriteLine($"BrowserContextPool 初始化完成，耗时: {sw2.ElapsedMilliseconds}ms");

Console.ReadKey();
Console.ReadKey();