using System.Diagnostics;
using MidjourneyAPI.Core.Database;
using MidjourneyAPI.Core.Headless;

// Furion: 添加 Inject() 方法，注入 Web 应用
var builder = WebApplication.CreateBuilder(args).Inject();

// Furion: Mvc 注入基础配置（带Swagger）
builder.Services.AddControllers().AddInject();

builder.Services.AddRedisCache();

// 初始化无头浏览器上下文池
var sw = new Stopwatch();
sw.Start();
builder.Services.AddBrowserContextPool(3, false);
sw.Stop();
Console.WriteLine($"BrowserContextPool 初始化完成，耗时: {sw.ElapsedMilliseconds}ms");

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

// Furion: 注入基础中间件（带Swagger）
app.UseInject(string.Empty);

app.MapControllers();

app.Run();