using System.Text.Json.Nodes;
using Furion.DynamicApiController;
using MidjourneyAPI.Core.Headless;
using NewLife.Caching;
using PuppeteerSharp;

namespace MidjourneyAPI.Core.Services;

public class MjAccountService(FullRedis redis, BrowserContextPool brsContextPool) : IDynamicApiController
{
    public List<MjAccount> GetAccounts()
    {
        var accounts = redis.Get<List<MjAccount>>("MjAccounts");
        return accounts ?? new List<MjAccount>();
    }

    public async Task<object> AddAccount(string token)
    {
        var mjAccount = new MjAccount { UserToken = token };

        if (redis.Get<MjAccount>("MjAccount:" + mjAccount.UserId.ToString("N")) != null)
        {
            return new
            {
                success = false, message = "Token已存在",
                mjAccount = redis.Get<MjAccount>("MjAccount:" + mjAccount.UserId.ToString("N"))
            };
        }

        var tempId = Guid.NewGuid().ToString("N");
        var page = await brsContextPool.GetDefaultPageByContextAsync(tempId);

        // 设置请求拦截器
        await page.SetRequestInterceptionAsync(true);
        page.Request += async (sender, e) =>
        {
            var headers = e.Request.Headers;
            headers["Accept"] = "*/*";
            headers["User-Agent"] = MidjourneyConst.UserAgent;
            headers["X-Csrf-Protection"] = "1";
            headers["Cookie"] = $"__Host-Midjourney.AuthUserToken={mjAccount.UserToken}";
            await e.Request.ContinueAsync(new Payload
            {
                Headers = headers
            });
        };

        // 携带cookie访问 https://www.midjourney.com/imagine
        await page.GoToAsync("https://www.midjourney.com/imagine", new NavigationOptions
        {
            WaitUntil = [WaitUntilNavigation.Networkidle2]
        });

        // 提取id=__NEXT_DATA__且type=application/json的script块中的内容
        var userDataScript = await page.QuerySelectorAsync("script#__NEXT_DATA__");
        if (userDataScript != null)
        {
            var innerTextHandle = await userDataScript.GetPropertyAsync("innerText");
            if (innerTextHandle != null)
            {
                var innerTextValue = await innerTextHandle.JsonValueAsync();
                var authUser = JsonNode.Parse(innerTextValue?.ToString() ?? string.Empty)?["props"]?["initialAuthUser"];
                if (authUser != null)
                {
                    mjAccount.UserJson = authUser.ToString();
                    mjAccount.UserId = authUser["id"] == null ? Guid.Empty : Guid.Parse(authUser["id"]!.ToString());
                    mjAccount.DisplayName = authUser["displayName"]?.ToString();
                    mjAccount.Email = authUser["email"]?.ToString();
                }
            }
        }

        if (mjAccount.UserId == Guid.Empty) return new { success = false, message = "添加账号失败", mjAccount };

        // 获取最新的 Cookies
        var latestCookies = await page.GetCookiesAsync();
        var cookiesDict = latestCookies.ToDictionary(c => c.Name, c => c.Value);
        mjAccount.UserToken = cookiesDict["__Host-Midjourney.AuthUserToken"];

        if (redis.Get<MjAccount>("MjAccount:" + mjAccount.UserId.ToString("N")) != null)
            return new { success = false, message = "账号已存在", mjAccount };

        redis.Set("MjAccount:" + mjAccount.UserId.ToString("N"), mjAccount);

        return new { success = true, message = "SUCCESS", mjAccount };
    }


    public async Task<object?> WhileAccountTest()
    {
        var success = false;
        while (!success)
        {
            try
            {
                var res = await TestAccount();
                success = true;
                return res;
            }
            catch
            {
                success = false;
            }
        }

        return null;
    }

    public async Task<object> TestAccount()
    {
        var userId = Guid.Parse("b6986ea9-ded7-41d6-903f-2cbdc398352d");
        var mjAccount = redis.Get<MjAccount>("MjAccount:" + userId.ToString("N"));

        if (mjAccount == null)
        {
            return new
            {
                success = false, message = "账号不已存在",
            };
        }

        var page = await brsContextPool.GetDefaultPageByContextAsync(Guid.NewGuid().ToString("N"));

        // 设置请求拦截器
        await page.SetRequestInterceptionAsync(true);
        page.Request += async (sender, e) =>
        {
            var headers = e.Request.Headers;
            headers["Accept"] = "*/*";
            headers["User-Agent"] = MidjourneyConst.UserAgent;
            headers["X-Csrf-Protection"] = "1";
            headers["Cookie"] = $"__Host-Midjourney.AuthUserToken={mjAccount.UserToken}";
            await e.Request.ContinueAsync(new Payload
            {
                Headers = headers
            });
        };

        // 携带cookie访问 https://www.midjourney.com/imagine
        await page.GoToAsync("https://www.midjourney.com/imagine", new NavigationOptions
        {
            WaitUntil = [WaitUntilNavigation.DOMContentLoaded]
        });

        // 提取id=__NEXT_DATA__且type=application/json的script块中的内容
        var userDataScript = await page.QuerySelectorAsync("script#__NEXT_DATA__");
        if (userDataScript != null)
        {
            var innerTextHandle = await userDataScript.GetPropertyAsync("innerText");
            if (innerTextHandle != null)
            {
                var innerTextValue = await innerTextHandle.JsonValueAsync();
                var authUser = JsonNode.Parse(innerTextValue?.ToString() ?? string.Empty)?["props"]?["initialAuthUser"];
                if (authUser != null)
                {
                    mjAccount.UserJson = authUser.ToString();
                    mjAccount.UserId = authUser["id"] == null ? Guid.Empty : Guid.Parse(authUser["id"]!.ToString());
                    mjAccount.DisplayName = authUser["displayName"]?.ToString();
                    mjAccount.Email = authUser["email"]?.ToString();
                }
            }
        }

        if (mjAccount.UserId == Guid.Empty) return new { success = false, message = "添加账号失败", mjAccount };

        // 获取最新的 Cookies
        var latestCookies = await page.GetCookiesAsync();
        var cookiesDict = latestCookies.ToDictionary(c => c.Name, c => c.Value);
        mjAccount.UserToken = cookiesDict["__Host-Midjourney.AuthUserToken"];

        await page.CloseAsync();
        if (redis.Get<MjAccount>("MjAccount:" + mjAccount.UserId.ToString("N")) != null)
            return new { success = false, message = "账号已存在", mjAccount };

        redis.Set("MjAccount:" + mjAccount.UserId.ToString("N"), mjAccount);

        return new { success = true, message = "SUCCESS", mjAccount };
    }
}